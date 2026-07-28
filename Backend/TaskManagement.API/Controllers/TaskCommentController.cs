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
        var comments = await _commentService.GetByTaskIdAsync(taskId);

        return Ok(comments);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid taskId,
        CreateTaskCommentDto dto)
    {
        var comment = await _commentService.CreateAsync(
            taskId,
            dto);

        return Ok(comment);
    }

    [HttpPut("{commentId:guid}")]
    public async Task<IActionResult> Update(
        Guid taskId,
        Guid commentId,
        UpdateTaskCommentDto dto)
    {
        var comment = await _commentService.UpdateAsync(
            commentId,
            dto);

        return Ok(comment);
    }

    [HttpDelete("{commentId:guid}")]
    public async Task<IActionResult> Delete(
        Guid taskId,
        Guid commentId)
    {
        await _commentService.DeleteAsync(commentId);

        return NoContent();
    }
}
