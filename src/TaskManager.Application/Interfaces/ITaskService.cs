
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Interfaces;

public interface ITaskService
{
    Task<TaskResponseDto> CreateAsync(CreateTaskDto dto, Guid userId);

    Task<List<TaskResponseDto>> GetAllAsync(Guid userId);

    Task<TaskResponseDto?> GetByIdAsync(Guid id, Guid userId);

    Task<TaskResponseDto?> UpdateAsync(Guid id, UpdateTaskDto dto, Guid userId);

    Task<bool> DeleteAsync(Guid id, Guid userId);
}
