namespace TaskManagement.API.Entities;

public class TaskAttachment
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }

    public TaskItem Task { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string ContentType { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}