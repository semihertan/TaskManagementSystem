using TaskManagement.API.DTOs.Task;
using TaskManagement.API.Services.Interfaces;
using AutoMapper;
using TaskManagement.API.Data;
using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Entities;

namespace TaskManagement.API.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
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

    public async Task<IEnumerable<TaskItemDto>> GetAllAsync(Guid userId)
    {
        var tasks = await _context.Tasks
        .Where(t => t.UserId == userId)
        .ToListAsync();

        return _mapper.Map<IEnumerable<TaskItemDto>>(tasks);
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