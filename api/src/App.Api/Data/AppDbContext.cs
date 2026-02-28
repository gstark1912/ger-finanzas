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

        // Seed months: 02/2026
        var m3 = new Guid("22222222-0000-0000-0000-000000000003");
        modelBuilder.Entity<Month>().HasData(
            new Month { Id = m3, Year = 2026, MonthNumber = 2 }
        );

        modelBuilder.Entity<FxRateMonth>().HasData(
            new FxRateMonth { Id = new Guid("33333333-0000-0000-0000-000000000001"), MonthId = m3, BaseCurrency = "USD", QuoteCurrency = "ARS", Rate = 1450m }
        );

        // Seed data
        var now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<ExpenseAccount>().HasData(
            new ExpenseAccount { Id = new Guid("11111111-0000-0000-0000-000000000001"), Name = "CASH $", Type = ExpenseAccountType.Cash, Currency = Currency.ARS, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ExpenseAccount { Id = new Guid("11111111-0000-0000-0000-000000000002"), Name = "Tarjeta Visa", Type = ExpenseAccountType.CC, Currency = Currency.ARS, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ExpenseAccount { Id = new Guid("11111111-0000-0000-0000-000000000003"), Name = "Tarjeta Amex", Type = ExpenseAccountType.CC, Currency = Currency.ARS, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ExpenseAccount { Id = new Guid("11111111-0000-0000-0000-000000000004"), Name = "Santander $", Type = ExpenseAccountType.Bank, Currency = Currency.ARS, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ExpenseAccount { Id = new Guid("11111111-0000-0000-0000-000000000005"), Name = "Citi USD", Type = ExpenseAccountType.Bank, Currency = Currency.USD, IsActive = true, CreatedAt = now, UpdatedAt = now }
        );

        var santander = new Guid("11111111-0000-0000-0000-000000000004");
        var cash = new Guid("11111111-0000-0000-0000-000000000001");
        var visa = new Guid("11111111-0000-0000-0000-000000000002");
        modelBuilder.Entity<FixedExpenseDefinition>().HasData(
            new FixedExpenseDefinition { Id = new Guid("44444444-0000-0000-0000-000000000001"), Name = "Osde", ExpenseAccountId = santander, Currency = Currency.ARS, IsActive = true, ExpireDay = 20, CreatedAt = now },
            new FixedExpenseDefinition { Id = new Guid("44444444-0000-0000-0000-000000000002"), Name = "Arba", ExpenseAccountId = santander, Currency = Currency.ARS, IsActive = true, ExpireDay = 10, CreatedAt = now },
            new FixedExpenseDefinition { Id = new Guid("44444444-0000-0000-0000-000000000003"), Name = "Contador", ExpenseAccountId = santander, Currency = Currency.ARS, IsActive = true, ExpireDay = 15, CreatedAt = now },
            new FixedExpenseDefinition { Id = new Guid("44444444-0000-0000-0000-000000000004"), Name = "Impuestos", ExpenseAccountId = santander, Currency = Currency.ARS, IsActive = true, ExpireDay = 20, CreatedAt = now },
            new FixedExpenseDefinition { Id = new Guid("44444444-0000-0000-0000-000000000005"), Name = "Personal", ExpenseAccountId = santander, Currency = Currency.ARS, IsActive = true, ExpireDay = 22, CreatedAt = now },
            new FixedExpenseDefinition { Id = new Guid("44444444-0000-0000-0000-000000000006"), Name = "Alquiler", ExpenseAccountId = cash, Currency = Currency.ARS, IsActive = true, ExpireDay = 10, CreatedAt = now },
            new FixedExpenseDefinition { Id = new Guid("44444444-0000-0000-0000-000000000007"), Name = "Tennis", ExpenseAccountId = cash, Currency = Currency.ARS, IsActive = true, ExpireDay = 8, CreatedAt = now },
            new FixedExpenseDefinition { Id = new Guid("44444444-0000-0000-0000-000000000008"), Name = "Gimnasio", ExpenseAccountId = cash, Currency = Currency.ARS, IsActive = true, ExpireDay = 10, CreatedAt = now },
            new FixedExpenseDefinition { Id = new Guid("44444444-0000-0000-0000-000000000009"), Name = "Seguro Moto", ExpenseAccountId = visa, Currency = Currency.ARS, IsActive = true, ExpireDay = null, CreatedAt = now },
            new FixedExpenseDefinition { Id = new Guid("44444444-0000-0000-0000-000000000010"), Name = "Seguro Auto", ExpenseAccountId = visa, Currency = Currency.ARS, IsActive = true, ExpireDay = null, CreatedAt = now },
            new FixedExpenseDefinition { Id = new Guid("44444444-0000-0000-0000-000000000011"), Name = "Hbo", ExpenseAccountId = visa, Currency = Currency.ARS, IsActive = true, ExpireDay = null, CreatedAt = now },
            new FixedExpenseDefinition { Id = new Guid("44444444-0000-0000-0000-000000000012"), Name = "Youtube", ExpenseAccountId = visa, Currency = Currency.USD, IsActive = true, ExpireDay = null, CreatedAt = now },
            new FixedExpenseDefinition { Id = new Guid("44444444-0000-0000-0000-000000000013"), Name = "Google Drive", ExpenseAccountId = visa, Currency = Currency.USD, IsActive = true, ExpireDay = null, CreatedAt = now },
            new FixedExpenseDefinition { Id = new Guid("44444444-0000-0000-0000-000000000014"), Name = "Spotify", ExpenseAccountId = visa, Currency = Currency.USD, IsActive = true, ExpireDay = null, CreatedAt = now },
            new FixedExpenseDefinition { Id = new Guid("44444444-0000-0000-0000-000000000015"), Name = "Netflix", ExpenseAccountId = visa, Currency = Currency.USD, IsActive = true, ExpireDay = null, CreatedAt = now }
        );

        var paidAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<FixedExpenseMonthEntry>().HasData(
            new FixedExpenseMonthEntry { Id = new Guid("55555555-0000-0000-0000-000000000001"), FixedExpenseDefinitionId = new Guid("44444444-0000-0000-0000-000000000001"), MonthId = m3, Amount = 235930m, PaidAt = paidAt },
            new FixedExpenseMonthEntry { Id = new Guid("55555555-0000-0000-0000-000000000002"), FixedExpenseDefinitionId = new Guid("44444444-0000-0000-0000-000000000004"), MonthId = m3, Amount = 437000m, PaidAt = paidAt },
            new FixedExpenseMonthEntry { Id = new Guid("55555555-0000-0000-0000-000000000003"), FixedExpenseDefinitionId = new Guid("44444444-0000-0000-0000-000000000008"), MonthId = m3, Amount = 49000m, PaidAt = paidAt },
            new FixedExpenseMonthEntry { Id = new Guid("55555555-0000-0000-0000-000000000004"), FixedExpenseDefinitionId = new Guid("44444444-0000-0000-0000-000000000007"), MonthId = m3, Amount = 95000m, PaidAt = paidAt },
            new FixedExpenseMonthEntry { Id = new Guid("55555555-0000-0000-0000-000000000005"), FixedExpenseDefinitionId = new Guid("44444444-0000-0000-0000-000000000006"), MonthId = m3, Amount = 1373000m, PaidAt = paidAt },
            new FixedExpenseMonthEntry { Id = new Guid("55555555-0000-0000-0000-000000000006"), FixedExpenseDefinitionId = new Guid("44444444-0000-0000-0000-000000000005"), MonthId = m3, Amount = 92000m, PaidAt = paidAt },
            new FixedExpenseMonthEntry { Id = new Guid("55555555-0000-0000-0000-000000000007"), FixedExpenseDefinitionId = new Guid("44444444-0000-0000-0000-000000000009"), MonthId = m3, Amount = 78285m, PaidAt = paidAt },
            new FixedExpenseMonthEntry { Id = new Guid("55555555-0000-0000-0000-000000000008"), FixedExpenseDefinitionId = new Guid("44444444-0000-0000-0000-000000000010"), MonthId = m3, Amount = 155358.33m, PaidAt = paidAt },
            new FixedExpenseMonthEntry { Id = new Guid("55555555-0000-0000-0000-000000000009"), FixedExpenseDefinitionId = new Guid("44444444-0000-0000-0000-000000000011"), MonthId = m3, Amount = 8628.35m, PaidAt = paidAt },
            new FixedExpenseMonthEntry { Id = new Guid("55555555-0000-0000-0000-000000000010"), FixedExpenseDefinitionId = new Guid("44444444-0000-0000-0000-000000000012"), MonthId = m3, Amount = 0m, PaidAt = paidAt },
            new FixedExpenseMonthEntry { Id = new Guid("55555555-0000-0000-0000-000000000011"), FixedExpenseDefinitionId = new Guid("44444444-0000-0000-0000-000000000013"), MonthId = m3, Amount = 0m, PaidAt = paidAt },
            new FixedExpenseMonthEntry { Id = new Guid("55555555-0000-0000-0000-000000000012"), FixedExpenseDefinitionId = new Guid("44444444-0000-0000-0000-000000000014"), MonthId = m3, Amount = 0m, PaidAt = paidAt },
            new FixedExpenseMonthEntry { Id = new Guid("55555555-0000-0000-0000-000000000013"), FixedExpenseDefinitionId = new Guid("44444444-0000-0000-0000-000000000015"), MonthId = m3, Amount = 0m, PaidAt = paidAt }
        );

        var saNow = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<SavingAccount>().HasData(
            new SavingAccount { Id = new Guid("66666666-0000-0000-0000-000000000001"), Name = "Santander", Type = SavingAccountType.Bank, Currency = Currency.USD, IsActive = true, CreatedAt = saNow, UpdatedAt = saNow },
            new SavingAccount { Id = new Guid("66666666-0000-0000-0000-000000000002"), Name = "Citi", Type = SavingAccountType.Bank, Currency = Currency.USD, IsActive = true, CreatedAt = saNow, UpdatedAt = saNow },
            new SavingAccount { Id = new Guid("66666666-0000-0000-0000-000000000003"), Name = "Wise", Type = SavingAccountType.Bank, Currency = Currency.USD, IsActive = true, CreatedAt = saNow, UpdatedAt = saNow },
            new SavingAccount { Id = new Guid("66666666-0000-0000-0000-000000000004"), Name = "Cash", Type = SavingAccountType.Cash, Currency = Currency.USD, IsActive = true, CreatedAt = saNow, UpdatedAt = saNow }
        );
        modelBuilder.Entity<SavingAccountMonth>().HasData(
            new SavingAccountMonth { Id = new Guid("77777777-0000-0000-0000-000000000001"), SavingAccountId = new Guid("66666666-0000-0000-0000-000000000001"), MonthId = m3, Balance = 20547.16m },
            new SavingAccountMonth { Id = new Guid("77777777-0000-0000-0000-000000000002"), SavingAccountId = new Guid("66666666-0000-0000-0000-000000000002"), MonthId = m3, Balance = 32027.61m },
            new SavingAccountMonth { Id = new Guid("77777777-0000-0000-0000-000000000003"), SavingAccountId = new Guid("66666666-0000-0000-0000-000000000003"), MonthId = m3, Balance = 0m },
            new SavingAccountMonth { Id = new Guid("77777777-0000-0000-0000-000000000004"), SavingAccountId = new Guid("66666666-0000-0000-0000-000000000004"), MonthId = m3, Balance = 12745m }
        );
    }
}
