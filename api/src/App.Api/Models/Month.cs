namespace App.Api.Models;

public class Month
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public int MonthNumber { get; set; }
    public FxRateMonth? FxRate { get; set; }
}

public class FxRateMonth
{
    public Guid Id { get; set; }
    public Guid MonthId { get; set; }
    public string BaseCurrency { get; set; } = "USD";
    public string QuoteCurrency { get; set; } = "ARS";
    public decimal Rate { get; set; }
    public Month Month { get; set; } = null!;
}

public record MonthWithFxRateDto(
    Guid Id,
    int Year,
    int MonthNumber,
    decimal? Rate
);

public record UpsertFxRateRequest(decimal Rate);
public record CreateMonthRequest(int Year, int Month);
