using TaskManagement.API.DTOs.TaskAttachment;

namespace TaskManagement.API.Services.Interfaces;

public interface ITaskAttachmentService
{
    Task<TaskAttachmentDto> UploadAsync(
        Guid taskId,
        CreateTaskAttachmentDto dto,
        Guid userId);

    Task<IEnumerable<TaskAttachmentDto>> GetByTaskIdAsync(
        Guid taskId,
        Guid userId);

    Task DeleteAsync(
        Guid attachmentId,
        Guid userId);

    Task<(byte[] FileBytes, string ContentType, string FileName)>DownloadAsync(Guid attachmentId, Guid userId);
}