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

            // CC cards with unpaid balance
            var unpaidCards = await db.CardBalanceMonths
                .Include(b => b.ExpenseAccount)
                .Where(b => b.Year == year && b.Month == month && !b.Paid)
                .Select(b => b.ExpenseAccount.Name)
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

            var summary = await DashboardCalculator.Calculate(db, [month], Currency.ARS);
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
            return Results.Ok(new { snapshot.Year, snapshot.MonthNumber, snapshot.ClosedAt });
        });
    }
}

public record CloseMonthRequest(int Year, int Month);
public record CloseCheckDto(List<string> UnpaidFixedExpenses, List<string> UnpaidCards);
