
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.DTOs.Auth;
using TaskManager.Application.Interfaces;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Identity;

namespace TaskManager.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly AppDbContext _dbContext;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IEmailVerificationService emailVerificationService,
        AppDbContext dbContext)
    {
        _userManager = userManager;
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
}
