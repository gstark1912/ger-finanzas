namespace App.Api.Models;

public class FixedExpenseDefinition
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid ExpenseAccountId { get; set; }
    public ExpenseAccount ExpenseAccount { get; set; } = null!;
    public Currency Currency { get; set; }
    public bool IsActive { get; set; } = true;
    public int? ExpireDay { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class FixedExpenseMonthEntry
{
    public Guid Id { get; set; }
    public Guid FixedExpenseDefinitionId { get; set; }
    public FixedExpenseDefinition Definition { get; set; } = null!;
    public Guid MonthId { get; set; }
    public Month Month { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; }
}

public record FixedExpenseDefinitionDto(
    Guid Id, string Name, Guid ExpenseAccountId, string Currency,
    bool IsActive, int? ExpireDay, DateTime CreatedAt
);

public record FixedExpenseMonthEntryDto(
    Guid Id, Guid FixedExpenseDefinitionId, Guid MonthId,
    decimal Amount, DateTime PaidAt
);

public record CreateFixedExpenseDefinitionRequest(
    string Name, Guid ExpenseAccountId, string Currency, int? ExpireDay
);

public record PayFixedExpenseRequest(decimal Amount);
