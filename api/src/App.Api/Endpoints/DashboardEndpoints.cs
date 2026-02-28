using Microsoft.EntityFrameworkCore;
using App.Api.Data;
using App.Api.Models;

namespace App.Api.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/dashboard/summary", async (AppDbContext db, string currency = "ARS", int lastMonths = 6) =>
        {
            if (!Enum.TryParse<Currency>(currency, true, out var targetCurrency))
                return Results.BadRequest(new { error = "currency must be ARS or USD" });

            var months = await db.Months
                .Include(m => m.FxRate)
                .OrderByDescending(m => m.Year).ThenByDescending(m => m.MonthNumber)
                .Take(lastMonths)
                .ToListAsync();

            var monthIds = months.Select(m => m.Id).ToList();

            var entries = await db.FixedExpenseMonthEntries
                .Include(e => e.Definition)
                    .ThenInclude(d => d.ExpenseAccount)
                .Where(e => monthIds.Contains(e.MonthId))
                .ToListAsync();

            // Group by account → month → sum (converted to target currency)
            var fixedExpenses = entries
                .GroupBy(e => e.Definition.ExpenseAccount)
                .Select(accountGroup =>
                {
                    var byMonth = months.Select(m =>
                    {
                        var rate = m.FxRate?.Rate ?? 0m;
                        var total = accountGroup
                            .Where(e => e.MonthId == m.Id)
                            .Sum(e =>
                            {
                                if (e.Definition.Currency == targetCurrency) return e.Amount;
                                if (targetCurrency == Currency.ARS) return e.Amount * rate;
                                return rate > 0 ? e.Amount / rate : 0m;
                            });
                        return new MonthTotal(m.Id, m.Year, m.MonthNumber, total);
                    }).ToList();

                    return new AccountFixedExpenseSummary(
                        accountGroup.Key.Id,
                        accountGroup.Key.Name,
                        byMonth
                    );
                })
                .ToList();

            var monthHeaders = months
                .OrderBy(m => m.Year).ThenBy(m => m.MonthNumber)
                .Select(m => new MonthWithFxRateDto(m.Id, m.Year, m.MonthNumber, m.FxRate?.Rate))
                .ToList();

            return Results.Ok(new DashboardSummaryDto(monthHeaders, fixedExpenses));
        })
        .WithTags("Dashboard");
    }
}

public record MonthTotal(Guid MonthId, int Year, int MonthNumber, decimal Total);
public record AccountFixedExpenseSummary(Guid AccountId, string AccountName, List<MonthTotal> Months);
public record DashboardSummaryDto(List<MonthWithFxRateDto> Months, List<AccountFixedExpenseSummary> FixedExpenses);
