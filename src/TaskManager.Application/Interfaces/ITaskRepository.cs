
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface ITaskRepository
{
    Task<TaskItem> CreateAsync(TaskItem task);

    Task<List<TaskItem>> GetAllAsync();

    Task<TaskItem?> GetByIdAsync(Guid id);

    Task<TaskItem?> UpdateAsync(TaskItem task);

    Task<bool> DeleteAsync(Guid id);
}
