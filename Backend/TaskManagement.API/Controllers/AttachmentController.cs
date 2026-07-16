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

    public TaskAttachmentsController(ITaskAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(Guid taskId, [FromForm] CreateTaskAttachmentDto dto)
    {
        var userId = Guid.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var attachment = await _attachmentService.UploadAsync(
            taskId,
            dto,
            userId);

        return Ok(attachment);
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid taskId)
    {
        var userId = Guid.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var attachments = await _attachmentService.GetByTaskIdAsync(
            taskId,
            userId);

        return Ok(attachments);
    }

    [HttpDelete("{attachmentId:guid}")]
    public async Task<IActionResult> Delete(Guid taskId, Guid attachmentId)
    {
        var userId = Guid.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

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
        var userId = Guid.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var result = await _attachmentService.DownloadAsync(
            attachmentId,
            userId);

        return File(
            result.FileBytes,
            result.ContentType,
            result.FileName);
    }
}