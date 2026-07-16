using TaskManagement.API.DTOs.Task;
using TaskManagement.API.Services.Interfaces;
using AutoMapper;
using TaskManagement.API.Data;
using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Entities;
using TaskManagement.API.Responses;

namespace TaskManagement.API.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private int totalCount;

    public TaskService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<TaskItemDto> CreateAsync(CreateTaskDto createTaskDto, Guid userId)
    {
        var task = _mapper.Map<TaskItem>(createTaskDto);

        task.UserId = userId;
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.Tasks.AddAsync(task);

        await _context.SaveChangesAsync();

        return _mapper.Map<TaskItemDto>(task);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (task == null)
            return false;

        _context.Tasks.Remove(task);

        await _context.SaveChangesAsync();

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
        var task = await _context.Tasks.
            FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if(task == null)
        {
            return null;
        }
        return _mapper.Map<TaskItemDto>(task);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateTaskDto updateTaskDto, Guid userId)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (task == null)
            return false;

        _mapper.Map(updateTaskDto, task);

        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}