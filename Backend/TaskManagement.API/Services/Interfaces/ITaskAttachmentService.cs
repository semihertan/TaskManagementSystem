using TaskManagement.API.DTOs.Task.TaskAttachment;
using TaskManagement.API.DTOs.TaskAttachment;

namespace TaskManagement.API.Services.Interfaces;

public interface ITaskAttachmentService
{
    Task<TaskAttachmentDto> UploadAsync(
        Guid taskId,
        CreateTaskAttachmentDto dto);

    Task<IEnumerable<TaskAttachmentDto>> GetByTaskIdAsync(
        Guid taskId);

    Task DeleteAsync(Guid attachmentId);

    Task<(byte[] FileBytes, string ContentType, string FileName)> DownloadAsync(Guid attachmentId);
}
