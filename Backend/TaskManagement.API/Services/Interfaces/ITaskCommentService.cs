using TaskManagement.API.DTOs.Task.TaskComment;

public interface ITaskCommentService
{
    Task<IEnumerable<TaskCommentDto>> GetByTaskIdAsync(Guid taskId, Guid userId);

    Task<TaskCommentDto> CreateAsync(
        Guid taskId,
        CreateTaskCommentDto createDto,
        Guid userId);

    Task<TaskCommentDto> UpdateAsync(
        Guid commentId,
        UpdateTaskCommentDto updateDto,
        Guid userId);

    Task DeleteAsync(
        Guid commentId,
        Guid userId);
}