namespace TaskManagement.API.DTOs.Task;

public class TaskFilterDto
{
	public int? Priority { get; set; }

	public int? Status { get; set; }

	public Guid? CategoryId { get; set; }

	public DateTime? DueDateFrom { get; set; }

	public DateTime? DueDateTo { get; set; }

	public string? Search { get; set; }

	public int Page {get; set;} = 1;

	public int PageSize {get; set;} = 10;

	public string? SortBy { get; set; }

	public string SortDirection { get; set; } = "desc";
}