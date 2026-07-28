using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;
using TaskManagement.API.DTOs.Task.TaskAttachment;
using TaskManagement.API.DTOs.TaskAttachment;
using TaskManagement.API.Entities;
using TaskManagement.API.Services.Interfaces;

namespace TaskManagement.API.Services.Implementations;

public class TaskAttachmentService : ITaskAttachmentService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public TaskAttachmentService(
        ApplicationDbContext context,
        IMapper mapper,
        ICurrentUserService currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task DeleteAsync(Guid attachmentId)
    {
        var attachment = await _context.TaskAttachments
            .Include(x => x.Task)
            .FirstOrDefaultAsync(x =>
                x.Id == attachmentId &&
                (_currentUser.IsAdmin || x.Task.UserId == _currentUser.UserId));

        if (attachment is null)
        {
            throw new KeyNotFoundException("Dosya bulunamadı.");
        }

        var fullPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            attachment.FilePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        _context.TaskAttachments.Remove(attachment);
        await _context.SaveChangesAsync();
    }

    public async Task<(
        byte[] FileBytes,
        string ContentType,
        string FileName)> DownloadAsync(
            Guid attachmentId)
    {
        var attachment = await _context.TaskAttachments
            .AsNoTracking()
            .Include(x => x.Task)
            .FirstOrDefaultAsync(x =>
                x.Id == attachmentId &&
                (_currentUser.IsAdmin || x.Task.UserId == _currentUser.UserId));

        if (attachment is null)
        {
            throw new KeyNotFoundException("Dosya bulunamadı.");
        }

        var fullPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            attachment.FilePath);

        if (!File.Exists(fullPath))
        {
            throw new KeyNotFoundException(
                "Dosya diskte bulunamadı.");
        }

        var fileBytes = await File.ReadAllBytesAsync(fullPath);

        return (
            fileBytes,
            attachment.ContentType,
            attachment.FileName
        );
    }

    public async Task<IEnumerable<TaskAttachmentDto>> GetByTaskIdAsync(
        Guid taskId)
    {
        var taskExists = await _context.Tasks
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id == taskId &&
                (_currentUser.IsAdmin || x.UserId == _currentUser.UserId));

        if (!taskExists)
        {
            throw new KeyNotFoundException("Görev bulunamadı.");
        }

        var attachments = await _context.TaskAttachments
            .AsNoTracking()
            .Where(x => x.TaskId == taskId)
            .OrderByDescending(x => x.UploadedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<TaskAttachmentDto>>(
            attachments);
    }

    public async Task<TaskAttachmentDto> UploadAsync(
        Guid taskId,
        CreateTaskAttachmentDto dto)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(x =>
                x.Id == taskId &&
                (_currentUser.IsAdmin || x.UserId == _currentUser.UserId));

        if (task is null)
        {
            throw new KeyNotFoundException("Görev bulunamadı.");
        }

        var uploadsFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Uploads");

        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName =
            $"{Guid.NewGuid()}{Path.GetExtension(dto.File.FileName)}";

        var relativePath = Path.Combine(
            "Uploads",
            uniqueFileName);

        var fullPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            relativePath);

        await using var stream = new FileStream(
            fullPath,
            FileMode.Create);

        await dto.File.CopyToAsync(stream);

        var attachment = new TaskAttachment
        {
            TaskId = taskId,
            FileName = dto.File.FileName,
            FilePath = relativePath,
            FileSize = dto.File.Length,
            ContentType = dto.File.ContentType,
            UploadedAt = DateTime.UtcNow
        };

        _context.TaskAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        return _mapper.Map<TaskAttachmentDto>(attachment);
    }
}
