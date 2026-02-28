using Microsoft.EntityFrameworkCore;
using App.Api.Models;

namespace App.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ExpenseAccount> ExpenseAccounts => Set<ExpenseAccount>();
    public DbSet<Month> Months => Set<Month>();
    public DbSet<FxRateMonth> FxRateMonths => Set<FxRateMonth>();

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

        // Seed months: 12/2025, 01/2026, 02/2026
        var m1 = new Guid("22222222-0000-0000-0000-000000000001");
        var m2 = new Guid("22222222-0000-0000-0000-000000000002");
        var m3 = new Guid("22222222-0000-0000-0000-000000000003");
        modelBuilder.Entity<Month>().HasData(
            new Month { Id = m1, Year = 2025, MonthNumber = 12 },
            new Month { Id = m2, Year = 2026, MonthNumber = 1 },
            new Month { Id = m3, Year = 2026, MonthNumber = 2 }
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
    }
}
