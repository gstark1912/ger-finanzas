namespace App.Api.Models;

public class ExpenseAccount
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ExpenseAccountType Type { get; set; }
    public Currency Currency { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum ExpenseAccountType
{
    Bank,
    Cash,
    CC
}

public enum Currency
{
    USD,
    ARS
}

public record ExpenseAccountDto(
    Guid Id,
    string Name,
    string Type,
    string Currency,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateExpenseAccountRequest(
    string Name,
    string Type,
    string Currency
);

public record UpdateExpenseAccountRequest(
    string Name,
    string Type,
    string Currency,
    bool IsActive
);
