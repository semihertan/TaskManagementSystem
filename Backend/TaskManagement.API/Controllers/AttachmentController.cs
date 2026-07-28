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

    [HttpPost]
    public async Task<IActionResult> Upload(
        Guid taskId,
        [FromForm] CreateTaskAttachmentDto dto)
    {
        var attachment = await _attachmentService.UploadAsync(
            taskId,
            dto);

        return Ok(attachment);
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid taskId)
    {
        var attachments = await _attachmentService.GetByTaskIdAsync(
            taskId);

        return Ok(attachments);
    }

    [HttpDelete("{attachmentId:guid}")]
    public async Task<IActionResult> Delete(
        Guid taskId,
        Guid attachmentId)
    {
        await _attachmentService.DeleteAsync(attachmentId);

        return NoContent();
    }

    [HttpGet("{attachmentId:guid}/download")]
    public async Task<IActionResult> Download(
        Guid taskId,
        Guid attachmentId)
    {
        var result = await _attachmentService.DownloadAsync(attachmentId);

        return File(
            result.FileBytes,
            result.ContentType,
            result.FileName);
    }
}
