using TaskManagement.API.DTOs.Task;
using TaskManagement.API.Services.Interfaces;
using AutoMapper;
using TaskManagement.API.Data;
using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Entities;
using TaskManagement.API.Responses;
using System.Formats.Asn1;
using TaskManagement.API.Enums;

namespace TaskManagement.API.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<TaskService> _logger;

    public TaskService(ApplicationDbContext context, IMapper mapper, ILogger<TaskService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }
    public async Task<TaskItemDto> CreateAsync(CreateTaskDto createTaskDto, Guid userId)
    {
        _logger.LogInformation("Creating task: {Title}", createTaskDto.Title);

        var task = _mapper.Map<TaskItem>(createTaskDto);

        task.UserId = userId;
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.Tasks.AddAsync(task);

        await _context.SaveChangesAsync();

            _logger.LogInformation(
        "Task created successfully. Id: {TaskId}",
        task.Id);

        return _mapper.Map<TaskItemDto>(task);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        _logger.LogInformation(
            "Deleting task with id: {TaskId}",
            id);

        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (task == null)
        {
            _logger.LogWarning(
            "Task not found for delete. Id: {TaskId}",
            id);

            return false;
        }

        _context.Tasks.Remove(task);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Task deleted successfully. Id: {TaskId}",
            id);

        return true;
    }

    public async Task<PagedResponse<TaskItemDto>> GetAllAsync(Guid userId, TaskFilterDto filterDto)
    {
        IQueryable<TaskItem> query = _context.Tasks
            .Where(t => t.UserId == userId);

        if (filterDto.Priority.HasValue)
        {
            query = query.Where(t => (int)t.Priority == filterDto.Priority.Value);
        }

        if (filterDto.Status.HasValue)
        {
            query = query.Where(t => (int)t.Status == filterDto.Status.Value);
        }

        if (filterDto.CategoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == filterDto.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterDto.Search))
        {
            query = query.Where(t =>
                t.Title.Contains(filterDto.Search) ||
                (t.Description != null && t.Description.Contains(filterDto.Search)));
        }

        if (filterDto.DueDateFrom.HasValue)
        {
            query = query.Where(t => t.DueDate >= filterDto.DueDateFrom.Value);
        }

        if (filterDto.DueDateTo.HasValue)
        {
            query = query.Where(t => t.DueDate <= filterDto.DueDateTo.Value);
        }

        if (filterDto.Page < 1)
            filterDto.Page = 1;

        if (filterDto.PageSize < 1)
            filterDto.PageSize = 10;
        
        var totalCount = await query.CountAsync();

        query = query
            .Skip((filterDto.Page - 1) * filterDto.PageSize)
            .Take(filterDto.PageSize);

        var tasks = await query.ToListAsync();

        var taskDtos = _mapper.Map<IEnumerable<TaskItemDto>>(tasks);

        return new PagedResponse<TaskItemDto>
        {
            Items = taskDtos,
            Page = filterDto.Page,
            PageSize = filterDto.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / filterDto.PageSize)
        };
    }

    public async Task<TaskItemDto?> GetByIdAsync(Guid id, Guid userId)
    {
        _logger.LogInformation("Getting task with id: {TaskId}", id);

        var task = await _context.Tasks.
            FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if(task == null)
        {
            _logger.LogWarning("Task not found. Id: {TaskId}", id);
            throw new KeyNotFoundException("Task bulunamadı.");
        }
        return _mapper.Map<TaskItemDto>(task);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateTaskDto updateTaskDto, Guid userId)
    {
        _logger.LogInformation(
            "Updating task. Id: {TaskId}",
            id);

        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (task == null)
        {
            _logger.LogWarning(
            "Task not found for update. Id: {TaskId}",
            id);

            return false;
        }

        _mapper.Map(updateTaskDto, task);

        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Task updated successfully. Id: {TaskId}",
            id);

        return true;
    }

    public async Task<TaskStatisticsDto> GetStatisticsAsync(Guid userId)
    {
        var totalTasks = await _context.Tasks
            .CountAsync(x => x.UserId == userId);

        var pendingTasks = await _context.Tasks
            .CountAsync(x => x.UserId == userId &&
                            x.Status == TaskItemStatus.Pending);

        var inProgressTasks = await _context.Tasks
            .CountAsync(x => x.UserId == userId &&
                            x.Status == TaskItemStatus.InProgress);

        var completedTasks = await _context.Tasks
            .CountAsync(x => x.UserId == userId &&
                            x.Status == TaskItemStatus.Completed);

        var cancelledTasks = await _context.Tasks
            .CountAsync(x => x.UserId == userId &&
                            x.Status == TaskItemStatus.Cancelled);

        var overdueTasks = await _context.Tasks
            .CountAsync(x => x.UserId == userId &&
                            x.DueDate < DateTime.UtcNow &&
                            x.Status != TaskItemStatus.Completed);

        var dueTodayTasks = await _context.Tasks
            .CountAsync(x => x.UserId == userId &&
                            x.DueDate.HasValue &&
                            x.DueDate.Value.Date == DateTime.UtcNow.Date);

        return new TaskStatisticsDto
        {
            TotalTasks = totalTasks,
            PendingTasks = pendingTasks,
            InProgressTasks = inProgressTasks,
            CompletedTasks = completedTasks,
            CancelledTasks = cancelledTasks,
            OverdueTasks = overdueTasks,
            DueTodayTasks = dueTodayTasks
        };
    }
    public async Task<IEnumerable<TaskItemDto>> GetOverdueTasksAsync(Guid userId)
    {
        var tasks = await _context.Tasks
            .Where(x =>
                x.UserId == userId &&
                x.DueDate.HasValue &&
                x.DueDate.Value < DateTime.UtcNow &&
                x.Status != TaskItemStatus.Completed &&
                x.Status != TaskItemStatus.Cancelled)
            .OrderBy(x => x.DueDate)
            .ToListAsync();

        return _mapper.Map<IEnumerable<TaskItemDto>>(tasks);
    }
}