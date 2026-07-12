using System.ComponentModel.DataAnnotations;

namespace TaskManagement.API.DTOs.Category;

public class UpdateCategoryDto
{
	[Required]
	[StringLength(100)]
	public string Name { get; set; } = string.Empty;

	[StringLength(500)]
	public string? Description { get; set; }

	[RegularExpression("^#([A-Fa-f0-9]{6})$")]
	public string Color { get; set; } = "#007bff";
}