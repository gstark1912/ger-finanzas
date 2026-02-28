namespace App.Api.Models;

public class SavingAccount
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SavingAccountType Type { get; set; }
    public Currency Currency { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum SavingAccountType { Bank, Cash }

public record SavingAccountDto(Guid Id, string Name, string Type, string Currency, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);
public record SaveSavingAccountRequest(string Name, string Type, string Currency, bool IsActive);

public class SavingAccountMonth
{
    public Guid Id { get; set; }
    public Guid SavingAccountId { get; set; }
    public SavingAccount SavingAccount { get; set; } = null!;
    public Guid MonthId { get; set; }
    public Month Month { get; set; } = null!;
    public decimal Balance { get; set; }
}

public record SavingAccountMonthDto(Guid Id, Guid SavingAccountId, Guid MonthId, decimal Balance);
public record UpsertSavingAccountMonthRequest(decimal Balance);

public class SavingAccountMonthTransaction
{
    public Guid Id { get; set; }
    public Guid SavingAccountMonthId { get; set; }
    public SavingAccountMonth SavingAccountMonth { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public string? Description { get; set; }
}

public record SavingAccountMonthTransactionDto(Guid Id, Guid SavingAccountMonthId, decimal Amount, DateOnly Date, string? Description);
public record CreateTransactionRequest(decimal Amount, DateOnly Date, string? Description);
