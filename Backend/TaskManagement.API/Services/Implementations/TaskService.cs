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
    private readonly ICurrentUserService _currentUser;

    public TaskService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<TaskService> logger,
        ICurrentUserService currentUser)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _currentUser = currentUser;
    }
    public async Task<TaskItemDto> CreateAsync(CreateTaskDto createTaskDto)
    {
        _logger.LogInformation("Creating task: {Title}", createTaskDto.Title);

        await EnsureCategoryAccessAsync(createTaskDto.CategoryId);

        var task = _mapper.Map<TaskItem>(createTaskDto);

        task.UserId = _currentUser.UserId;
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.Tasks.AddAsync(task);

        await _context.SaveChangesAsync();

            _logger.LogInformation(
        "Task created successfully. Id: {TaskId}",
        task.Id);

        return _mapper.Map<TaskItemDto>(task);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation(
            "Deleting task with id: {TaskId}",
            id);

        var task = await _context.Tasks
            .FirstOrDefaultAsync(t =>
                t.Id == id &&
                (_currentUser.IsAdmin || t.UserId == _currentUser.UserId));

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

    public async Task<PagedResponse<TaskItemDto>> GetAllAsync(TaskFilterDto filterDto)
    {
        IQueryable<TaskItem> query = _context.Tasks
            .AsNoTracking();

        if (!_currentUser.IsAdmin)
        {
            query = query.Where(t => t.UserId == _currentUser.UserId);
        }

        if (filterDto.Priority.HasValue)
        {
            query = query.Where(
                t => (int)t.Priority == filterDto.Priority.Value
            );
        }

        if (filterDto.Status.HasValue)
        {
            query = query.Where(
                t => (int)t.Status == filterDto.Status.Value
            );
        }

        if (filterDto.CategoryId.HasValue)
        {
            query = query.Where(
                t => t.CategoryId == filterDto.CategoryId.Value
            );
        }

        if (!string.IsNullOrWhiteSpace(filterDto.Search))
        {
            var searchPattern =
                $"%{filterDto.Search.Trim()}%";

            query = query.Where(t =>
                EF.Functions.ILike(
                    t.Title,
                    searchPattern
                ) ||
                (
                    t.Description != null &&
                    EF.Functions.ILike(
                        t.Description,
                        searchPattern
                    )
                )
            );
        }

        if (filterDto.DueDateFrom.HasValue)
        {
            var dueDateFrom =
                filterDto.DueDateFrom.Value.Date;

            query = query.Where(
                t => t.DueDate >= dueDateFrom
            );
        }

        if (filterDto.DueDateTo.HasValue)
        {
            var dueDateToExclusive =
                filterDto.DueDateTo.Value.Date.AddDays(1);

            query = query.Where(
                t => t.DueDate < dueDateToExclusive
            );
        }

        if (filterDto.Page < 1)
        {
            filterDto.Page = 1;
        }

        if (filterDto.PageSize < 1)
        {
            filterDto.PageSize = 10;
        }

        var totalCount = await query.CountAsync();

        var isDescending =
            string.Equals(
                filterDto.SortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase
            );

        query = filterDto.SortBy?.Trim().ToLowerInvariant() switch
        {
            "title" => isDescending
                ? query
                    .OrderByDescending(t => t.Title)
                    .ThenByDescending(t => t.CreatedAt)
                : query
                    .OrderBy(t => t.Title)
                    .ThenBy(t => t.CreatedAt),

            "priority" => isDescending
                ? query
                    .OrderByDescending(t => t.Priority)
                    .ThenByDescending(t => t.CreatedAt)
                : query
                    .OrderBy(t => t.Priority)
                    .ThenBy(t => t.CreatedAt),

            "status" => isDescending
                ? query
                    .OrderByDescending(t => t.Status)
                    .ThenByDescending(t => t.CreatedAt)
                : query
                    .OrderBy(t => t.Status)
                    .ThenBy(t => t.CreatedAt),

            "duedate" => isDescending
                ? query
                    .OrderByDescending(t => t.DueDate)
                    .ThenByDescending(t => t.CreatedAt)
                : query
                    .OrderBy(t => t.DueDate)
                    .ThenBy(t => t.CreatedAt),

            "createdat" => isDescending
                ? query.OrderByDescending(t => t.CreatedAt)
                : query.OrderBy(t => t.CreatedAt),

            "updatedat" => isDescending
                ? query
                    .OrderByDescending(t => t.UpdatedAt)
                    .ThenByDescending(t => t.CreatedAt)
                : query
                    .OrderBy(t => t.UpdatedAt)
                    .ThenBy(t => t.CreatedAt),

            _ => query
                .OrderByDescending(t => t.UpdatedAt)
                .ThenByDescending(t => t.CreatedAt)
        };

        var tasks = await query
            .Skip((filterDto.Page - 1) * filterDto.PageSize)
            .Take(filterDto.PageSize)
            .ToListAsync();

        var taskDtos =
            _mapper.Map<IEnumerable<TaskItemDto>>(tasks);

        return new PagedResponse<TaskItemDto>
        {
            Items = taskDtos,
            Page = filterDto.Page,
            PageSize = filterDto.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(
                (double)totalCount / filterDto.PageSize
            )
        };
    }

    public async Task<TaskItemDto?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting task with id: {TaskId}", id);

        var task = await _context.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.Id == id &&
                (_currentUser.IsAdmin || t.UserId == _currentUser.UserId));

        if(task == null)
        {
            throw new KeyNotFoundException("Task bulunamadı.");
        }
        return _mapper.Map<TaskItemDto>(task);
    }

    public async Task<TaskItemDto> UpdateAsync(Guid id, UpdateTaskDto updateTaskDto)
    {
        _logger.LogInformation(
            "Updating task. Id: {TaskId}",
            id);

        var task = await _context.Tasks
            .FirstOrDefaultAsync(t =>
                t.Id == id &&
                (_currentUser.IsAdmin || t.UserId == _currentUser.UserId));

        if (task == null)
        {
            _logger.LogWarning(
                "Task not found for update. Id: {TaskId}",
                id);

            throw new KeyNotFoundException(
                "Görev bulunamadı."
            );
        }

        await EnsureCategoryAccessAsync(updateTaskDto.CategoryId);

        _mapper.Map(updateTaskDto, task);

        task.Priority =
            (Priority)updateTaskDto.Priority;

        task.Status =
            (TaskItemStatus)updateTaskDto.Status;

        if (task.Status == TaskItemStatus.Completed)
        {
            task.CompletedAt ??= DateTime.UtcNow;
        }
        else
        {
            task.CompletedAt = null;
        }

        task.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Task status before save. Id: {TaskId}, Status: {Status}, StatusValue: {StatusValue}",
            task.Id,
            task.Status,
            (int)task.Status);

        await _context.SaveChangesAsync();

        return _mapper.Map<TaskItemDto>(task);
    }

    public async Task<TaskStatisticsDto> GetStatisticsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var now = DateTime.UtcNow;

        IQueryable<TaskItem> query = _context.Tasks;

        if (!_currentUser.IsAdmin)
        {
            query = query.Where(x => x.UserId == _currentUser.UserId);
        }

        var statistics = await query
            .GroupBy(x => 1)
            .Select(g => new TaskStatisticsDto
            {
                TotalTasks = g.Count(),

                PendingTasks = g.Count(x =>
                    x.Status == TaskItemStatus.Pending),

                InProgressTasks = g.Count(x =>
                    x.Status == TaskItemStatus.InProgress),

                CompletedTasks = g.Count(x =>
                    x.Status == TaskItemStatus.Completed),

                CancelledTasks = g.Count(x =>
                    x.Status == TaskItemStatus.Cancelled),

                OverdueTasks = g.Count(x =>
                    x.DueDate < now &&
                    x.Status != TaskItemStatus.Completed),

                DueTodayTasks = g.Count(x =>
                    x.DueDate.HasValue &&
                    x.DueDate.Value.Date == today)
            })
            .FirstOrDefaultAsync();

        return statistics ?? new TaskStatisticsDto();
    }
    
    public async Task<IEnumerable<TaskItemDto>> GetOverdueTasksAsync()
    {
        IQueryable<TaskItem> query = _context.Tasks.AsNoTracking();

        if (!_currentUser.IsAdmin)
        {
            query = query.Where(x => x.UserId == _currentUser.UserId);
        }

        var tasks = await query
            .Where(x =>
                x.DueDate.HasValue &&
                x.DueDate.Value < DateTime.UtcNow &&
                x.Status != TaskItemStatus.Completed &&
                x.Status != TaskItemStatus.Cancelled)
            .OrderBy(x => x.DueDate)
            .ToListAsync();

        return _mapper.Map<IEnumerable<TaskItemDto>>(tasks);
    }

    private async Task EnsureCategoryAccessAsync(Guid? categoryId)
    {
        if (!categoryId.HasValue)
        {
            return;
        }

        var categoryExists = await _context.Categories
            .AsNoTracking()
            .AnyAsync(category =>
                category.Id == categoryId.Value &&
                (_currentUser.IsAdmin || category.UserId == _currentUser.UserId));

        if (!categoryExists)
        {
            throw new KeyNotFoundException("Kategori bulunamadı.");
        }
    }
}
