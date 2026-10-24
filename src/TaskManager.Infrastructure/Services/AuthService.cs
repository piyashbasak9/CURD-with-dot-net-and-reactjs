
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.DTOs.Auth;
using TaskManager.Application.Interfaces;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

namespace TaskManager.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly JwtOptions _jwtOptions;
    private readonly AppDbContext _dbContext;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IEmailVerificationService emailVerificationService,
        IOptions<JwtOptions> jwtOptions,
        AppDbContext dbContext)
    {
        _userManager = userManager;
        _jwtOptions = jwtOptions.Value;
        _emailVerificationService = emailVerificationService;
        _dbContext = dbContext;
    }

    public async Task RegisterAsync(
        RegisterDto dto,
        CancellationToken cancellationToken = default)
    {
        var email = dto.Email.Trim();

        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            Name = dto.Name.Trim(),
            PhoneNumber = dto.PhoneNumber.Trim(),
            Address = string.IsNullOrWhiteSpace(dto.Address)
                ? null
                : dto.Address.Trim(),
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(
            user,
            dto.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(error => error.Description));

            throw new InvalidOperationException(errors);
        }

        await _emailVerificationService.SendCodeAsync(
            user.Id,
            user.Email!,
            cancellationToken);
    }

    public async Task VerifyEmailAsync(
    VerifyEmailDto dto,
    CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email.Trim());

        if (user is null)
        {
            throw new InvalidOperationException(
                "Invalid email or verification code.");
        }

        if (user.EmailConfirmed)
        {
            throw new InvalidOperationException(
                "This email has already been verified.");
        }

        await _emailVerificationService.VerifyCodeAsync(
            user.Id,
            dto.Code,
            cancellationToken);

        var token = await _userManager
            .GenerateEmailConfirmationTokenAsync(user);

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(error => error.Description));

            throw new InvalidOperationException(errors);
        }
    }

    public async Task<LoginResponseDto> LoginAsync(
        LoginDto dto,
        CancellationToken cancellationToken = default)
    {
        var email = dto.Email.Trim();

        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            throw new InvalidOperationException(
                "Invalid email or password.");
        }

        if (!user.EmailConfirmed)
        {
            throw new InvalidOperationException(
                "Please verify your email before logging in.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(
            user,
            dto.Password);

        if (!passwordValid)
        {
            throw new InvalidOperationException(
                "Invalid email or password.");
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtOptions.Key));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name)
        };

        var expiresAt = now.AddMinutes(
            _jwtOptions.AccessTokenLifetimeMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresAtUtc = expiresAt
        };
    }
}
