using TaskManagement.API.Enums;
namespace TaskManagement.API.Entities;

public class TaskItem
{
    public Guid Id { get; set; }

    public User User { get; set; } = null!;

    public Guid? CategoryId { get; set; }

    public Category? Category { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Priority Priority { get; set; } = Priority.Normal;

    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;

    public DateTime? DueDate { get; set; }

    public DateTime? CompletedAt { get; set; }

    public Guid UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();

    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}