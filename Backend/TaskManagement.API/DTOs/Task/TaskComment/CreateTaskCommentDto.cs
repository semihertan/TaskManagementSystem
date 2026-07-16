using System.ComponentModel.DataAnnotations;

public class CreateTaskCommentDto
{
    [Required]
    [StringLength(1000)]
    public string Content { get; set; } = string.Empty;
}