using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagement.API.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks/{taskId:guid}/comments")]
public class TaskCommentsController : ControllerBase
{
    private readonly ITaskCommentService _commentService;

    public TaskCommentsController(ITaskCommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid taskId)
    {
        var userId = Guid.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var comments = await _commentService.GetByTaskIdAsync(taskId, userId);

        return Ok(comments);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid taskId,
        CreateTaskCommentDto dto)
    {
        var userId = Guid.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var comment = await _commentService.CreateAsync(
            taskId,
            dto,
            userId);

        return Ok(comment);
    }

    [HttpPut("{commentId:guid}")]
    public async Task<IActionResult> Update(
        Guid taskId,
        Guid commentId,
        UpdateTaskCommentDto dto)
    {
        var userId = Guid.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var comment = await _commentService.UpdateAsync(
            commentId,
            dto,
            userId);

        return Ok(comment);
    }

    [HttpDelete("{commentId:guid}")]
    public async Task<IActionResult> Delete(
        Guid taskId,
        Guid commentId)
    {
        var userId = Guid.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        await _commentService.DeleteAsync(
            commentId,
            userId);

        return NoContent();
    }
}