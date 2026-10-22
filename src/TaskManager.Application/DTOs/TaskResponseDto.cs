
namespace TaskManager.Application.DTOs;

public class TaskResponseDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsCompleted { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
}
