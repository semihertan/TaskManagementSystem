public class TaskStatisticsDto
{
    public int TotalTasks { get; set; }

    public int PendingTasks { get; set; }

    public int InProgressTasks { get; set; }

    public int CompletedTasks { get; set; }

    public int CancelledTasks { get; set; }

    public int OverdueTasks { get; set; }

    public int DueTodayTasks { get; set; }
}