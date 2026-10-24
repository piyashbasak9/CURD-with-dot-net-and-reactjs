
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskResponseDto> CreateAsync(
        CreateTaskDto dto,
        Guid userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new ArgumentException("Task title is required.");
        }

        var task = new TaskItem
        {
            Title = dto.Title.Trim(),
            Description = dto.Description,
            UserId = userId
        };

        var createdTask = await _taskRepository.CreateAsync(task);

        return MapToResponseDto(createdTask);
    }

    public async Task<List<TaskResponseDto>> GetAllAsync(Guid userId)
    {
        var tasks = await _taskRepository.GetAllAsync(userId);

        return tasks.Select(MapToResponseDto).ToList();
    }

    public async Task<TaskResponseDto?> GetByIdAsync(
        Guid id,
        Guid userId)
    {
        var task = await _taskRepository.GetByIdAsync(id, userId);

        if (task is null)
        {
            return null;
        }

        return MapToResponseDto(task);
    }

    public async Task<TaskResponseDto?> UpdateAsync(
        Guid id,
        UpdateTaskDto dto,
        Guid userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new ArgumentException("Task title is required.");
        }

        var task = new TaskItem
        {
            Id = id,
            Title = dto.Title.Trim(),
            Description = dto.Description,
            IsCompleted = dto.IsCompleted,
            UserId = userId
        };

        var updatedTask = await _taskRepository.UpdateAsync(task, userId);

        if (updatedTask is null)
        {
            return null;
        }

        return MapToResponseDto(updatedTask);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        return await _taskRepository.DeleteAsync(id, userId);
    }

    private static TaskResponseDto MapToResponseDto(TaskItem task)
    {
        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            CreatedAtUtc = task.CreatedAtUtc
        };
    }
}
