
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
                "Email verification HMAC key must be a valid hexadecimal string.");
        }

        if (_hmacKey.Length != 32)
        {
            throw new InvalidOperationException(
                "Email verification HMAC key must be exactly 32 bytes.");
        }

        if (_options.CodeLifetimeMinutes <= 0)
        {
            throw new InvalidOperationException(
                "Email verification code lifetime must be greater than zero.");
        }
    }

    public async Task SendCodeAsync(
        Guid userId,
        string email,
        CancellationToken cancellationToken = default)
    {
        // Generate a random 6-digit verification code.
        var code = RandomNumberGenerator
            .GetInt32(0, 1_000_000)
            .ToString("D6");

        // Store a keyed hash instead of the actual verification code.
        var codeHash = CreateCodeHash(userId, code);
        var now = DateTimeOffset.UtcNow;

        // Remove previous unused verification codes for this user.
        var oldCodes = await _dbContext.EmailVerificationCodes
            .Where(x => x.UserId == userId && x.UsedAtUtc == null)
            .ToListAsync(cancellationToken);

        _dbContext.EmailVerificationCodes.RemoveRange(oldCodes);

        // Create a new verification code with an expiration time.
        var verification = new EmailVerificationCode
        {
            UserId = userId,
            CodeHash = codeHash,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddMinutes(
                _options.CodeLifetimeMinutes)
        };

        _dbContext.EmailVerificationCodes.Add(verification);

        await _dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            // Send the actual code to the user's email.
            await _emailService.SendEmailAsync(
                email,
                "Verify your Task Manager account",
                $"Your verification code is: {code}\n\n" +
                $"This code expires in {_options.CodeLifetimeMinutes} minutes.",
                cancellationToken);
        }
        catch
        {
            // Remove the code record if sending the email fails.
            _dbContext.EmailVerificationCodes.Remove(verification);

            await _dbContext.SaveChangesAsync(cancellationToken);

            throw;
        }
    }

    private string CreateCodeHash(Guid userId, string code)
    {
        var data = Encoding.UTF8.GetBytes(
            $"{userId:N}:{code}");

        var hash = HMACSHA256.HashData(_hmacKey, data);

        return Convert.ToHexString(hash);
    }
}
