using TaskManagement.API.DTOs.Task;
using TaskManagement.API.Responses;
public interface ITaskService
{
    Task<PagedResponse<TaskItemDto>> GetAllAsync(TaskFilterDto filterDto);
    Task<TaskItemDto?> GetByIdAsync(Guid id);
    Task<TaskItemDto> CreateAsync(CreateTaskDto createTaskDto);
    Task<TaskItemDto> UpdateAsync(Guid id, UpdateTaskDto updateTaskDto);
    Task<bool> DeleteAsync(Guid id);
    Task<TaskStatisticsDto> GetStatisticsAsync();
    Task<IEnumerable<TaskItemDto>> GetOverdueTasksAsync();
}
