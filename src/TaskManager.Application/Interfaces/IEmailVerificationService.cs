namespace TaskManager.Application.Interfaces;

public interface IEmailVerificationService
{
    Task SendCodeAsync(
        Guid userId,
        string email,
        CancellationToken cancellationToken = default);
}