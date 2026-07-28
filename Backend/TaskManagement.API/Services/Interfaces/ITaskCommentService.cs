using TaskManagement.API.DTOs.Task.TaskComment;

public interface ITaskCommentService
{
    Task<IEnumerable<TaskCommentDto>> GetByTaskIdAsync(Guid taskId);

    Task<TaskCommentDto> CreateAsync(
        Guid taskId,
        CreateTaskCommentDto createDto);

    Task<TaskCommentDto> UpdateAsync(
        Guid commentId,
        UpdateTaskCommentDto updateDto);

    Task DeleteAsync(Guid commentId);
}
