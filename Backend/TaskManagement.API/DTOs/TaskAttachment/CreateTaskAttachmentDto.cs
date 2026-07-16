using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TaskManagement.API.DTOs.TaskAttachment;

public class CreateTaskAttachmentDto
{
    [Required]
    public IFormFile File { get; set; } = null!;
}