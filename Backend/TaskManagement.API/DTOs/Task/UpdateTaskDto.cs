using System.ComponentModel.DataAnnotations;

namespace TaskManagement.API.DTOs.Task;

public class UpdateTaskDto
{
	[Required]
	[StringLength(200)]
	public string Title { get; set; } = string.Empty;

	[StringLength(2000)]
	public string? Description { get; set; }

	[Range(1, 5)]
	public int Priority { get; set; }

	[Range(0, 3)]
	public int Status { get; set; }

	public DateTime? DueDate { get; set; }

	public Guid? CategoryId { get; set; }
}