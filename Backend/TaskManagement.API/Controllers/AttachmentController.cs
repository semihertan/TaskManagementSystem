using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.DTOs.TaskAttachment;
using TaskManagement.API.Services.Interfaces;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/tasks/{taskId:guid}/attachments")]
[Authorize]
public class TaskAttachmentsController : ControllerBase
{
    private readonly ITaskAttachmentService _attachmentService;

    public TaskAttachmentsController(
        ITaskAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    private Guid GetUserId()
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            throw new UnauthorizedAccessException(
                "Geçersiz kullanıcı bilgisi.");
        }

        return userId;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(
        Guid taskId,
        [FromForm] CreateTaskAttachmentDto dto)
    {
        var userId = GetUserId();

        var attachment = await _attachmentService.UploadAsync(
            taskId,
            dto,
            userId);

        return Ok(attachment);
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid taskId)
    {
        var userId = GetUserId();

        var attachments = await _attachmentService.GetByTaskIdAsync(
            taskId,
            userId);

        return Ok(attachments);
    }

    [HttpDelete("{attachmentId:guid}")]
    public async Task<IActionResult> Delete(
        Guid taskId,
        Guid attachmentId)
    {
        var userId = GetUserId();

        await _attachmentService.DeleteAsync(
            attachmentId,
            userId);

        return NoContent();
    }

    [HttpGet("{attachmentId:guid}/download")]
    public async Task<IActionResult> Download(
        Guid taskId,
        Guid attachmentId)
    {
        var userId = GetUserId();

        var result = await _attachmentService.DownloadAsync(
            attachmentId,
            userId);

        return File(
            result.FileBytes,
            result.ContentType,
            result.FileName);
    }
}