
using TaskManager.Application.DTOs.Auth;
using TaskManager.Application.Interfaces;

namespace TaskManager.Application.Interfaces;

public interface IAuthService
{
    Task RegisterAsync(
        RegisterDto dto,
        CancellationToken cancellationToken = default);
}
