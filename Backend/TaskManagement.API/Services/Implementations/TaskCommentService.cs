using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;
using TaskManagement.API.DTOs.Task.TaskComment;
using TaskManagement.API.Entities;

namespace TaskManagement.API.Services.Implementations;

public class TaskCommentService : ITaskCommentService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public TaskCommentService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TaskCommentDto> CreateAsync(
        Guid taskId,
        CreateTaskCommentDto createDto,
        Guid userId)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(x => x.Id == taskId && x.UserId == userId);

        if (task == null)
        {
            throw new Exception("Görev bulunamadı.");
        }

        var comment = _mapper.Map<TaskComment>(createDto);

        comment.TaskId = taskId;
        comment.UserId = userId;
        comment.CreatedAt = DateTime.UtcNow;

        _context.TaskComments.Add(comment);

        await _context.SaveChangesAsync();

        return _mapper.Map<TaskCommentDto>(comment);
    }

    public async Task DeleteAsync(Guid commentId, Guid userId)
    {
        var comment = await _context.TaskComments
            .FirstOrDefaultAsync(x =>
                x.Id == commentId &&
                x.UserId == userId);

        if (comment == null)
        {
            throw new Exception("Yorum bulunamadı.");
        }

        _context.TaskComments.Remove(comment);

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TaskCommentDto>> GetByTaskIdAsync(Guid taskId, Guid userId)
    {
        var task = await _context.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == taskId && x.UserId == userId);

        if (task == null)
        {
            throw new Exception("Görev bulunamadı.");
        }

        var comments = await _context.TaskComments
            .Where(x => x.TaskId == taskId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<TaskCommentDto>>(comments);
    }

    public async Task<TaskCommentDto> UpdateAsync(
        Guid commentId,
        UpdateTaskCommentDto updateDto,
        Guid userId)
    {
        var comment = await _context.TaskComments
            .FirstOrDefaultAsync(x =>
                x.Id == commentId &&
                x.UserId == userId);

        if (comment == null)
        {
            throw new Exception("Yorum bulunamadı.");
        }

        _mapper.Map(updateDto, comment);

        comment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return _mapper.Map<TaskCommentDto>(comment);
    }
}