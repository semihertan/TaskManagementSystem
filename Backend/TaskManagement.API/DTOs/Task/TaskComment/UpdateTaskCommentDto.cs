using System.ComponentModel.DataAnnotations;

public class UpdateTaskCommentDto
{
    [Required]
    [StringLength(1000)]
    public string Content { get; set; } = string.Empty;
}