using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TaskManagement.API.DTOs.Task;
using TaskManagement.API.Responses;

namespace TaskManagement.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TaskFilterDto filterDto)
    {
        var userId = Guid.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var tasks = await _taskService.GetAllAsync(userId, filterDto);

        return Ok(new ApiResponse<PagedResponse<TaskItemDto>>
        {
            Success = true,
            Message = "Görevler başarıyla getirildi.",
            Data = tasks
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var task = await _taskService.GetByIdAsync(id, userId);

    if (task == null)
    {
        return NotFound(new ApiResponse<object>
        {
            Success = false,
            Message = "Görev bulunamadı.",
            Data = null
        });
    }

        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskDto createTaskDto)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var createdTask = await _taskService.CreateAsync(createTaskDto, userId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdTask.Id },
            new ApiResponse<TaskItemDto>
            {
                Success = true,
                Message = "Görev başarıyla oluşturuldu.",
                Data = createdTask
            });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTaskDto updateTaskDto)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var updated = await _taskService.UpdateAsync(id, updateTaskDto, userId);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var deleted = await _taskService.DeleteAsync(id, userId);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}