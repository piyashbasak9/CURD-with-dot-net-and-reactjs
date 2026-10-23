
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Email;

namespace TaskManager.Infrastructure.Services;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly AppDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly EmailVerificationOptions _options;
    private readonly byte[] _hmacKey;

    public EmailVerificationService(
        AppDbContext dbContext,
        IEmailService emailService,
        IOptions<EmailVerificationOptions> options)
    {
        _dbContext = dbContext;
        _emailService = emailService;
        _options = options.Value;

        try
        {
            _hmacKey = Convert.FromHexString(_options.HmacKey);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException(
                "Email verification HMAC key must be valid hexadecimal.");
        }

        if (_hmacKey.Length != 32)
        {
            throw new InvalidOperationException(
                "Email verification HMAC key must be exactly 32 bytes.");
        }

        if (_options.CodeLifetimeMinutes <= 0)
        {
            throw new InvalidOperationException(
                "Code lifetime must be greater than zero.");
        }
    }

    public async Task SendCodeAsync(
        Guid userId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var code = RandomNumberGenerator
            .GetInt32(0, 1_000_000)
            .ToString("D6");

        var verification = new EmailVerificationCode
        {
            UserId = userId,
            CodeHash = CreateCodeHash(userId, code),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(
                _options.CodeLifetimeMinutes)
        };

        var oldCodes = await _dbContext.EmailVerificationCodes
            .Where(x => x.UserId == userId && x.UsedAtUtc == null)
            .ToListAsync(cancellationToken);

        _dbContext.EmailVerificationCodes.RemoveRange(oldCodes);
        _dbContext.EmailVerificationCodes.Add(verification);

        await _dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailService.SendEmailAsync(
                email,
                "Verify your Task Manager account",
                $"Your verification code is: {code}\n\n" +
                $"This code expires in {_options.CodeLifetimeMinutes} minutes.",
                cancellationToken);
        }
        catch
        {
            _dbContext.EmailVerificationCodes.Remove(verification);
            await _dbContext.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    public async Task VerifyCodeAsync(
        Guid userId,
        string code,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var verification = await _dbContext.EmailVerificationCodes
            .Where(x =>
                x.UserId == userId &&
                x.UsedAtUtc == null &&
                x.ExpiresAtUtc > now)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (verification is null)
        {
            throw new InvalidOperationException(
                "Verification code is invalid or expired. Please request a new code.");
        }

        var submittedHash = CreateCodeHash(userId, code);

        var expectedBytes = Convert.FromHexString(verification.CodeHash);
        var submittedBytes = Convert.FromHexString(submittedHash);

        if (!CryptographicOperations.FixedTimeEquals(
                expectedBytes,
                submittedBytes))
        {
            throw new InvalidOperationException(
                "Invalid verification code.");
        }

        verification.UsedAtUtc = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private string CreateCodeHash(Guid userId, string code)
    {
        var data = Encoding.UTF8.GetBytes($"{userId:N}:{code}");
        var hash = HMACSHA256.HashData(_hmacKey, data);

        return Convert.ToHexString(hash);
    }
}
