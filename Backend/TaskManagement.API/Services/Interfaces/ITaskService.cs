using TaskManagement.API.DTOs.Task;
using TaskManagement.API.Responses;
public interface ITaskService
{
    Task<PagedResponse<TaskItemDto>> GetAllAsync(Guid userId, TaskFilterDto filterDto);
    Task<TaskItemDto?> GetByIdAsync(Guid id, Guid userId);
    Task<TaskItemDto> CreateAsync(CreateTaskDto createTaskDto, Guid userId);
    Task<TaskItemDto> UpdateAsync(Guid id, UpdateTaskDto updateTaskDto, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
    Task<TaskStatisticsDto> GetStatisticsAsync(Guid userId);
    Task<IEnumerable<TaskItemDto>> GetOverdueTasksAsync(Guid userId);
}