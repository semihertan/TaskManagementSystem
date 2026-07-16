namespace TaskManagement.API.DTOs.TaskAttachment;

public class TaskAttachmentDto
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string ContentType { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; }
}