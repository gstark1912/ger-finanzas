namespace App.Api.Models;

public class InvestmentAccount
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Currency Currency { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal? ExpectedAnnualReturnPct { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public record InvestmentAccountDto(Guid Id, string Name, string Currency, bool IsActive, decimal? ExpectedAnnualReturnPct, DateTime CreatedAt, DateTime UpdatedAt);
public record SaveInvestmentAccountRequest(string Name, string Currency, bool IsActive, decimal? ExpectedAnnualReturnPct);

public class InvestmentAccountMonth
{
    public Guid Id { get; set; }
    public Guid InvestmentAccountId { get; set; }
    public InvestmentAccount InvestmentAccount { get; set; } = null!;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal Balance { get; set; }
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
}

public record InvestmentAccountMonthDto(Guid Id, Guid InvestmentAccountId, int Month, int Year, decimal Balance, decimal Income, decimal Expenses, bool Exists);
public record UpsertInvestmentAccountMonthRequest(decimal Balance, decimal Income, decimal Expenses);
