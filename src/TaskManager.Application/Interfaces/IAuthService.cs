
using TaskManager.Application.DTOs.Auth;

namespace TaskManager.Application.Interfaces;

public interface IAuthService
{
    Task RegisterAsync(
        RegisterDto dto,
        CancellationToken cancellationToken = default);

    Task VerifyEmailAsync(
        VerifyEmailDto dto,
        CancellationToken cancellationToken = default);

    Task<LoginResponseDto> LoginAsync(
      LoginDto dto,
      CancellationToken cancellationToken = default);
}
