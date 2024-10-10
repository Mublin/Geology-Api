namespace Geology_Api.Models;

public class AdminLog
{
    public int AdminLogId { get; set; }
    public string Action { get; set; }
    public int UserId { get; set; }
    public int PerformedBy { get; set; }
    public DateTime DatePerformed { get; set; } = DateTime.UtcNow;
}
