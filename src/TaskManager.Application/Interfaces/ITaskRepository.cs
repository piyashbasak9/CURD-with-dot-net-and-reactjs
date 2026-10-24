
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface ITaskRepository
{
    Task<TaskItem> CreateAsync(TaskItem task);

    Task<List<TaskItem>> GetAllAsync(Guid userId);

    Task<TaskItem?> GetByIdAsync(Guid id, Guid userId);

    Task<TaskItem?> UpdateAsync(TaskItem task, Guid userId);

    Task<bool> DeleteAsync(Guid id, Guid userId);
}
