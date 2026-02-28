namespace App.Api.Models;

public class CardInstallment
{
    public Guid Id { get; set; }
    public Guid ExpenseAccountId { get; set; }
    public ExpenseAccount ExpenseAccount { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public Currency Currency { get; set; }
    public int Payments { get; set; } = 0;
    public int Installments { get; set; } = 1;
    public bool Active { get; set; } = true;
    public DateOnly Date { get; set; }
    public int StartingMonth { get; set; } = 0;
    public List<CardExpenseMonth> CardExpenseMonths { get; set; } = [];
}

public class CardExpenseMonth
{
    public Guid Id { get; set; }
    public Guid CardInstallmentId { get; set; }
    public CardInstallment CardInstallment { get; set; } = null!;
    public decimal Total { get; set; }
    public Currency Currency { get; set; }
    public int Installment { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public bool Paid { get; set; } = false;
}

public record CreateCardInstallmentRequest(
    Guid ExpenseAccountId,
    string Description,
    decimal Total,
    string Currency,
    int Installments,
    DateOnly Date,
    int StartingMonth
);

public record CardInstallmentDto(
    Guid Id,
    Guid ExpenseAccountId,
    string ExpenseAccountName,
    string Description,
    decimal Total,
    string Currency,
    int Payments,
    int Installments,
    bool Active,
    DateOnly Date,
    int StartingMonth,
    List<CardExpenseMonthDto> CardExpenseMonths
);

public record CardExpenseMonthDto(Guid Id, Guid CardInstallmentId, decimal Total, string Currency, int Installment, int Month, int Year, bool Paid);
