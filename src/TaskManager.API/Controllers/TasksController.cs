
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponseDto>> Create(
        CreateTaskDto dto)
    {
        try
        {
            var createdTask = await _taskService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdTask.Id },
                createdTask);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<TaskResponseDto>>> GetAll()
    {
        var tasks = await _taskService.GetAllAsync();

        return Ok(tasks);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskResponseDto>> GetById(Guid id)
    {
        var task = await _taskService.GetByIdAsync(id);

        if (task is null)
        {
            return NotFound();
        }

        return Ok(task);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskResponseDto>> Update(
        Guid id,
        UpdateTaskDto dto)
    {
        try
        {
            var updatedTask = await _taskService.UpdateAsync(id, dto);

            if (updatedTask is null)
            {
                return NotFound();
            }

            return Ok(updatedTask);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _taskService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
