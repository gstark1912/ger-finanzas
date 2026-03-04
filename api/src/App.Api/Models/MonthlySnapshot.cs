namespace App.Api.Models;

public class MonthlySnapshot
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public int MonthNumber { get; set; }
    public DateTime ClosedAt { get; set; }
    public decimal FxRate { get; set; }
    public string SummaryJson { get; set; } = string.Empty;
}
