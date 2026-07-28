using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;
using TaskManagement.API.DTOs.Task.TaskComment;
using TaskManagement.API.Entities;
using TaskManagement.API.Services.Interfaces;

namespace TaskManagement.API.Services.Implementations;

public class TaskCommentService : ITaskCommentService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public TaskCommentService(
        ApplicationDbContext context,
        IMapper mapper,
        ICurrentUserService currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<TaskCommentDto> CreateAsync(
        Guid taskId,
        CreateTaskCommentDto createDto)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(x =>
                x.Id == taskId &&
                (_currentUser.IsAdmin || x.UserId == _currentUser.UserId));

        if (task is null)
        {
            throw new KeyNotFoundException(
                "Görev bulunamadı.");
        }

        var comment = _mapper.Map<TaskComment>(createDto);

        comment.TaskId = taskId;
        comment.UserId = _currentUser.UserId;
        comment.CreatedAt = DateTime.UtcNow;

        _context.TaskComments.Add(comment);
        await _context.SaveChangesAsync();

        return _mapper.Map<TaskCommentDto>(comment);
    }

    public async Task DeleteAsync(Guid commentId)
    {
        var comment = await _context.TaskComments
            .Include(x => x.Task)
            .FirstOrDefaultAsync(x =>
                x.Id == commentId &&
                (_currentUser.IsAdmin ||
                    (x.UserId == _currentUser.UserId &&
                     x.Task.UserId == _currentUser.UserId)));

        if (comment is null)
        {
            throw new KeyNotFoundException(
                "Yorum bulunamadı.");
        }

        _context.TaskComments.Remove(comment);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TaskCommentDto>>
        GetByTaskIdAsync(
            Guid taskId)
    {
        var taskExists = await _context.Tasks
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id == taskId &&
                (_currentUser.IsAdmin || x.UserId == _currentUser.UserId));

        if (!taskExists)
        {
            throw new KeyNotFoundException(
                "Görev bulunamadı.");
        }

        var comments = await _context.TaskComments
            .AsNoTracking()
            .Where(x => x.TaskId == taskId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<TaskCommentDto>>(
            comments);
    }

    public async Task<TaskCommentDto> UpdateAsync(
        Guid commentId,
        UpdateTaskCommentDto updateDto)
    {
        var comment = await _context.TaskComments
            .Include(x => x.Task)
            .FirstOrDefaultAsync(x =>
                x.Id == commentId &&
                (_currentUser.IsAdmin ||
                    (x.UserId == _currentUser.UserId &&
                     x.Task.UserId == _currentUser.UserId)));

        if (comment is null)
        {
            throw new KeyNotFoundException(
                "Yorum bulunamadı.");
        }

        _mapper.Map(updateDto, comment);

        comment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return _mapper.Map<TaskCommentDto>(comment);
    }
}
