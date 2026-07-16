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

    public TaskAttachmentService(
        ApplicationDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task DeleteAsync(Guid attachmentId, Guid userId)
    {
        var attachment = await _context.TaskAttachments
            .Include(x => x.Task)
            .FirstOrDefaultAsync(x =>
                x.Id == attachmentId &&
                x.Task.UserId == userId);

        if(attachment == null)
        {
            throw new Exception("Dosya bulunamadı");   
        }

        if (File.Exists(attachment.FilePath))
        {
            File.Delete(attachment.FilePath);
        }

        _context.TaskAttachments.Remove(attachment);

        await _context.SaveChangesAsync();
    }

    public async Task<(byte[] FileBytes, string ContentType, string FileName)> DownloadAsync(Guid attachmentId, Guid userId)
    {
        var attachment = await _context.TaskAttachments
            .Include(x => x.Task)
            .FirstOrDefaultAsync(x =>
                x.Id == attachmentId &&
                x.Task.UserId == userId);

        if (attachment == null)
        {
            throw new Exception("Dosya bulunamadı.");
        }

        if (!File.Exists(attachment.FilePath))
        {
            throw new Exception("Dosya diskte bulunamadı.");
        }

        var fullPath = Path.Combine(Directory.GetCurrentDirectory(),
            attachment.FilePath);

        var fileBytes = await File.ReadAllBytesAsync(fullPath);

        return (
            fileBytes,
            attachment.ContentType,
            attachment.FileName
        );
    }

    public async Task<IEnumerable<TaskAttachmentDto>> GetByTaskIdAsync(Guid taskId, Guid userId)
    {
        var attachments = await _context.TaskAttachments
            .Include(x => x.Task)
            .Where(x => x.TaskId == taskId && x.Task.UserId == userId)
            .ToListAsync();

        return _mapper.Map<IEnumerable<TaskAttachmentDto>>(attachments);
    }

    public async Task<TaskAttachmentDto> UploadAsync(Guid taskId, CreateTaskAttachmentDto dto, Guid userId)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

        if (task == null)
        {
            throw new Exception("Görev bulunamadı.");
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

        using var stream = new FileStream(
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