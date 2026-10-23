
using Microsoft.AspNetCore.Identity;
using TaskManager.Application.DTOs.Auth;
using TaskManager.Application.Interfaces;
using TaskManager.Infrastructure.Identity;

namespace TaskManager.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
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
    }
}
