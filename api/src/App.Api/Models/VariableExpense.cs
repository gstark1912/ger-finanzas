namespace App.Api.Models;

public class VariableExpense
{
    public Guid Id { get; set; }
    public Guid ExpenseAccountId { get; set; }
    public ExpenseAccount ExpenseAccount { get; set; } = null!;
    public decimal Total { get; set; }
    public Currency Currency { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}

public record UpsertVariableExpenseRequest(decimal Total);
public record VariableExpenseDto(Guid Id, Guid ExpenseAccountId, decimal Total, string Currency, int Month, int Year);
