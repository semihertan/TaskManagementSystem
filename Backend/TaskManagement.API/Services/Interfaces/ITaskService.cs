using TaskManagement.API.DTOs.Task;
public interface ITaskService
{
    Task<IEnumerable<TaskItemDto>> GetAllAsync(Guid userId, TaskFilterDto filterDto);
    Task<TaskItemDto?> GetByIdAsync(Guid id, Guid userId);
    Task<TaskItemDto> CreateAsync(CreateTaskDto createTaskDto, Guid userId);
    Task<bool> UpdateAsync(Guid id, UpdateTaskDto updateTaskDto, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}