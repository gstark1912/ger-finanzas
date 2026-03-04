using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using App.Api.Data;
using App.Api.Models;

namespace App.Api.Endpoints;

public static class MonthlySnapshotEndpoints
{
    public static void MapMonthlySnapshotEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/monthly-snapshots").WithTags("MonthlySnapshots");

        group.MapGet("/{year:int}/{month:int}", async (int year, int month, AppDbContext db) =>
        {
            var snapshot = await db.MonthlySnapshots
                .FirstOrDefaultAsync(s => s.Year == year && s.MonthNumber == month);
            if (snapshot is null) return Results.NotFound();
            var summary = JsonSerializer.Deserialize<DashboardSummaryDto>(snapshot.SummaryJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return Results.Ok(new { snapshot.Year, snapshot.MonthNumber, snapshot.ClosedAt, snapshot.FxRate, summary });
        });

        group.MapGet("/close-check/{year:int}/{month:int}", async (int year, int month, AppDbContext db) =>
        {
            var monthEntity = await db.Months.FirstOrDefaultAsync(m => m.Year == year && m.MonthNumber == month);
            if (monthEntity is null) return Results.NotFound(new { error = "Month not found" });

            // Fixed expenses: definitions active with no entry for this month
            var allDefinitions = await db.FixedExpenseDefinitions
                .Include(d => d.ExpenseAccount)
                .Where(d => d.IsActive)
                .ToListAsync();
            var paidDefinitionIds = await db.FixedExpenseMonthEntries
                .Where(e => e.MonthId == monthEntity.Id)
                .Select(e => e.FixedExpenseDefinitionId)
                .ToListAsync();
            var unpaidFixed = allDefinitions
                .Where(d => !paidDefinitionIds.Contains(d.Id))
                .Select(d => d.Name)
                .ToList();

            // CC cards with unpaid balance: has expenses for the month but no CardBalanceMonth with Paid = true
            var ccAccountIds = await db.ExpenseAccounts
                .Where(a => a.Type == ExpenseAccountType.CC && a.IsActive)
                .Select(a => a.Id)
                .ToListAsync();

            var paidCardIds = await db.CardBalanceMonths
                .Where(b => b.Year == year && b.Month == month && b.Paid)
                .Select(b => b.ExpenseAccountId)
                .ToListAsync();

            var cardsWithExpenses = await db.CardExpenseMonths
                .Where(e => e.Year == year && e.Month == month && e.Total > 0
                    && ccAccountIds.Contains(e.CardInstallment.ExpenseAccountId))
                .Select(e => e.CardInstallment.ExpenseAccountId)
                .Distinct()
                .ToListAsync();

            var unpaidCardIds = cardsWithExpenses.Where(id => !paidCardIds.Contains(id)).ToList();
            var unpaidCards = await db.ExpenseAccounts
                .Where(a => unpaidCardIds.Contains(a.Id))
                .Select(a => a.Name)
                .ToListAsync();

            return Results.Ok(new CloseCheckDto(unpaidFixed, unpaidCards));
        });

        group.MapPost("/close", async (CloseMonthRequest req, AppDbContext db) =>
        {
            var existing = await db.MonthlySnapshots
                .FirstOrDefaultAsync(s => s.Year == req.Year && s.MonthNumber == req.Month);
            if (existing is not null)
                return Results.Conflict(new { error = "Month already closed" });

            var month = await db.Months
                .Include(m => m.FxRate)
                .FirstOrDefaultAsync(m => m.Year == req.Year && m.MonthNumber == req.Month);
            if (month is null) return Results.NotFound(new { error = "Month not found" });

            var summary = await DashboardCalculator.Calculate(db, [month], Currency.USD);
            var fxRate = month.FxRate?.Rate ?? 0m;

            var snapshot = new MonthlySnapshot
            {
                Id = Guid.NewGuid(),
                Year = req.Year,
                MonthNumber = req.Month,
                ClosedAt = DateTime.UtcNow,
                FxRate = fxRate,
                SummaryJson = JsonSerializer.Serialize(summary)
            };
            db.MonthlySnapshots.Add(snapshot);

            // Create next month if it doesn't exist
            var (nextYear, nextMonth) = req.Month == 12 ? (req.Year + 1, 1) : (req.Year, req.Month + 1);
            var nextExists = await db.Months.AnyAsync(m => m.Year == nextYear && m.MonthNumber == nextMonth);
            if (!nextExists)
            {
                var nextMonthEntity = new Month { Id = Guid.NewGuid(), Year = nextYear, MonthNumber = nextMonth };
                db.Months.Add(nextMonthEntity);
                db.FxRateMonths.Add(new FxRateMonth
                {
                    Id = Guid.NewGuid(),
                    MonthId = nextMonthEntity.Id,
                    BaseCurrency = "USD",
                    QuoteCurrency = "ARS",
                    Rate = fxRate
                });
            }

            await db.SaveChangesAsync();

            // Carry forward saving account balances to next month
            var savingMonths = await db.SavingAccountMonths
                .Where(s => s.MonthId == month.Id)
                .ToListAsync();
            var nextMonthDb = await db.Months.FirstOrDefaultAsync(m => m.Year == nextYear && m.MonthNumber == nextMonth);
            if (nextMonthDb != null)
            {
                foreach (var sam in savingMonths)
                {
                    var exists = await db.SavingAccountMonths.AnyAsync(s => s.SavingAccountId == sam.SavingAccountId && s.MonthId == nextMonthDb.Id);
                    if (!exists)
                        db.SavingAccountMonths.Add(new SavingAccountMonth { Id = Guid.NewGuid(), SavingAccountId = sam.SavingAccountId, MonthId = nextMonthDb.Id, Balance = sam.Balance });
                }
                await db.SaveChangesAsync();
            }

            return Results.Ok(new { snapshot.Year, snapshot.MonthNumber, snapshot.ClosedAt });
        });
    }
}

public record CloseMonthRequest(int Year, int Month);
public record CloseCheckDto(List<string> UnpaidFixedExpenses, List<string> UnpaidCards);
