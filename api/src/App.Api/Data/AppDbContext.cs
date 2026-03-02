using Microsoft.EntityFrameworkCore;
using App.Api.Models;

namespace App.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ExpenseAccount> ExpenseAccounts => Set<ExpenseAccount>();
    public DbSet<Month> Months => Set<Month>();
    public DbSet<FxRateMonth> FxRateMonths => Set<FxRateMonth>();
    public DbSet<FixedExpenseDefinition> FixedExpenseDefinitions => Set<FixedExpenseDefinition>();
    public DbSet<FixedExpenseMonthEntry> FixedExpenseMonthEntries => Set<FixedExpenseMonthEntry>();
    public DbSet<SavingAccount> SavingAccounts => Set<SavingAccount>();
    public DbSet<SavingAccountMonth> SavingAccountMonths => Set<SavingAccountMonth>();
    public DbSet<SavingAccountMonthTransaction> SavingAccountMonthTransactions => Set<SavingAccountMonthTransaction>();
    public DbSet<CardInstallment> CardInstallments => Set<CardInstallment>();
    public DbSet<CardExpenseMonth> CardExpenseMonths => Set<CardExpenseMonth>();
    public DbSet<CardBalanceMonth> CardBalanceMonths => Set<CardBalanceMonth>();
    public DbSet<VariableExpense> VariableExpenses => Set<VariableExpense>();
    public DbSet<InvestmentAccount> InvestmentAccounts => Set<InvestmentAccount>();
    public DbSet<InvestmentAccountMonth> InvestmentAccountMonths => Set<InvestmentAccountMonth>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExpenseAccount>(entity =>
        {
            entity.ToTable("expense_accounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Currency).HasConversion<string>();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });

        modelBuilder.Entity<Month>(entity =>
        {
            entity.ToTable("months");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.FxRate).WithOne(e => e.Month).HasForeignKey<FxRateMonth>(e => e.MonthId);
        });

        modelBuilder.Entity<FxRateMonth>(entity =>
        {
            entity.ToTable("fx_rate_months");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Rate).HasPrecision(18, 4);
        });

        modelBuilder.Entity<FixedExpenseDefinition>(entity =>
        {
            entity.ToTable("fixed_expense_definitions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Currency).HasConversion<string>();
            entity.HasOne(e => e.ExpenseAccount).WithMany().HasForeignKey(e => e.ExpenseAccountId);
        });

        modelBuilder.Entity<FixedExpenseMonthEntry>(entity =>
        {
            entity.ToTable("fixed_expense_month_entries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.HasOne(e => e.Definition).WithMany().HasForeignKey(e => e.FixedExpenseDefinitionId);
            entity.HasOne(e => e.Month).WithMany().HasForeignKey(e => e.MonthId);
            entity.HasIndex(e => new { e.FixedExpenseDefinitionId, e.MonthId }).IsUnique();
        });

        modelBuilder.Entity<SavingAccount>(entity =>
        {
            entity.ToTable("saving_accounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Currency).HasConversion<string>();
        });

        modelBuilder.Entity<SavingAccountMonth>(entity =>
        {
            entity.ToTable("saving_account_months");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Balance).HasPrecision(18, 2);
            entity.HasOne(e => e.SavingAccount).WithMany().HasForeignKey(e => e.SavingAccountId);
            entity.HasOne(e => e.Month).WithMany().HasForeignKey(e => e.MonthId);
            entity.HasIndex(e => new { e.SavingAccountId, e.MonthId }).IsUnique();
        });

        modelBuilder.Entity<SavingAccountMonthTransaction>(entity =>
        {
            entity.ToTable("saving_account_month_transactions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.HasOne(e => e.SavingAccountMonth).WithMany().HasForeignKey(e => e.SavingAccountMonthId);
        });

        modelBuilder.Entity<CardInstallment>(entity =>
        {
            entity.ToTable("card_installments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasConversion<string>();
            entity.HasOne(e => e.ExpenseAccount).WithMany().HasForeignKey(e => e.ExpenseAccountId);
        });

        modelBuilder.Entity<CardExpenseMonth>(entity =>
        {
            entity.ToTable("card_expense_months");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasConversion<string>();
            entity.HasOne(e => e.CardInstallment).WithMany(c => c.CardExpenseMonths).HasForeignKey(e => e.CardInstallmentId);
        });

        modelBuilder.Entity<VariableExpense>(entity =>
        {
            entity.ToTable("variable_expenses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasConversion<string>();
            entity.HasOne(e => e.ExpenseAccount).WithMany().HasForeignKey(e => e.ExpenseAccountId);
            entity.HasIndex(e => new { e.ExpenseAccountId, e.Month, e.Year }).IsUnique();
        });

        modelBuilder.Entity<InvestmentAccount>(entity =>
        {
            entity.ToTable("investment_accounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Currency).HasConversion<string>();
            entity.Property(e => e.ExpectedAnnualReturnPct).HasPrecision(8, 4);
        });

        modelBuilder.Entity<InvestmentAccountMonth>(entity =>
        {
            entity.ToTable("investment_account_months");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Balance).HasPrecision(18, 2);
            entity.Property(e => e.Income).HasPrecision(18, 2);
            entity.Property(e => e.Expenses).HasPrecision(18, 2);
            entity.HasOne(e => e.InvestmentAccount).WithMany().HasForeignKey(e => e.InvestmentAccountId);
            entity.HasIndex(e => new { e.InvestmentAccountId, e.Month, e.Year }).IsUnique();
        });

        modelBuilder.Entity<CardBalanceMonth>(entity =>
        {
            entity.ToTable("card_balance_months");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OtherExpensesArs).HasPrecision(18, 2);
            entity.Property(e => e.OtherExpensesUsd).HasPrecision(18, 2);
            entity.HasOne(e => e.ExpenseAccount).WithMany().HasForeignKey(e => e.ExpenseAccountId);
            entity.HasIndex(e => new { e.ExpenseAccountId, e.Month, e.Year }).IsUnique();
        });
    }
}
