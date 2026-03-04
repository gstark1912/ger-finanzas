using Microsoft.EntityFrameworkCore;
using System.Text.Json;
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

            var snapshots = await db.MonthlySnapshots
                .Where(s => months.Select(m => m.Year * 100 + m.MonthNumber).Contains(s.Year * 100 + s.MonthNumber))
                .ToListAsync();

            var closedMonthKeys = snapshots.Select(s => s.Year * 100 + s.MonthNumber).ToHashSet();

            // Months that need live calculation
            var liveMonths = months.Where(m => !closedMonthKeys.Contains(m.Year * 100 + m.MonthNumber)).ToList();

            DashboardSummaryDto? liveSummary = null;
            if (liveMonths.Count > 0)
                liveSummary = await DashboardCalculator.Calculate(db, liveMonths, targetCurrency);

            // Merge snapshot data with live data per month
            var allMonthHeaders = months
                .OrderBy(m => m.Year).ThenBy(m => m.MonthNumber)
                .Select(m =>
                {
                    var snap = snapshots.FirstOrDefault(s => s.Year == m.Year && s.MonthNumber == m.MonthNumber);
                    return new MonthWithFxRateDto(m.Id, m.Year, m.MonthNumber, snap?.FxRate ?? m.FxRate?.Rate, snap != null);
                })
                .ToList();

            var mergedFixedExpenses = MergeGroups(months, snapshots, liveSummary, targetCurrency, s => s.FixedExpenses);
            var mergedVariableExpenses = MergeGroups(months, snapshots, liveSummary, targetCurrency, s => s.VariableExpenses);
            var mergedSavings = MergeGroups(months, snapshots, liveSummary, targetCurrency, s => s.Savings);
            var mergedInvestments = MergeGroups(months, snapshots, liveSummary, targetCurrency, s => s.Investments);

            var now = DateTime.UtcNow;
            var currentMonth = months.FirstOrDefault(m => m.Year == now.Year && m.MonthNumber == now.Month)
                ?? months.OrderByDescending(m => m.Year).ThenByDescending(m => m.MonthNumber).First();

            decimal kpiArs, kpiUsd;
            var currentSnap = snapshots.FirstOrDefault(s => s.Year == currentMonth.Year && s.MonthNumber == currentMonth.MonthNumber);
            if (currentSnap != null)
            {
                var snapSummary = JsonSerializer.Deserialize<DashboardSummaryDto>(currentSnap.SummaryJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                kpiArs = snapSummary.KpiCostoMensualArs;
                kpiUsd = snapSummary.KpiCostoMensualUsd;
            }
            else
            {
                kpiArs = liveSummary?.KpiCostoMensualArs ?? 0m;
                kpiUsd = liveSummary?.KpiCostoMensualUsd ?? 0m;
            }

            return Results.Ok(new DashboardSummaryDto(allMonthHeaders, mergedFixedExpenses, mergedSavings, mergedVariableExpenses, mergedInvestments, kpiArs, kpiUsd));
        })
        .WithTags("Dashboard");
    }

    private static List<AccountFixedExpenseSummary> MergeGroups(
        List<Month> months,
        List<MonthlySnapshot> snapshots,
        DashboardSummaryDto? liveSummary,
        Currency targetCurrency,
        Func<DashboardSummaryDto, List<AccountFixedExpenseSummary>> selector)
    {
        var result = new Dictionary<Guid, AccountFixedExpenseSummary>();

        // Add live months data
        if (liveSummary != null)
        {
            foreach (var acc in selector(liveSummary))
            {
                result[acc.AccountId] = acc;
            }
        }

        // Merge snapshot months into each account
        foreach (var snap in snapshots)
        {
            var snapSummary = JsonSerializer.Deserialize<DashboardSummaryDto>(snap.SummaryJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            var month = months.First(m => m.Year == snap.Year && m.MonthNumber == snap.MonthNumber);

            foreach (var acc in selector(snapSummary))
            {
                var snapMonthTotal = acc.Months.FirstOrDefault(mt => mt.Year == snap.Year && mt.MonthNumber == snap.MonthNumber);
                if (snapMonthTotal == null) continue;

                // Remap MonthId to the actual Month.Id from our months list
                var remapped = snapMonthTotal with { MonthId = month.Id };

                if (result.TryGetValue(acc.AccountId, out var existing))
                {
                    var updatedMonths = existing.Months.Where(mt => mt.MonthId != month.Id).Append(remapped).ToList();
                    result[acc.AccountId] = existing with { Months = updatedMonths };
                }
                else
                {
                    result[acc.AccountId] = acc with { Months = [remapped] };
                }
            }
        }

        // Ensure all accounts have entries for all months (fill missing with 0)
        var monthIds = months.Select(m => m.Id).ToList();
        return result.Values.Select(acc =>
        {
            var filledMonths = months.OrderBy(m => m.Year).ThenBy(m => m.MonthNumber).Select(m =>
                acc.Months.FirstOrDefault(mt => mt.MonthId == m.Id) ?? new MonthTotal(m.Id, m.Year, m.MonthNumber, 0m)
            ).ToList();
            return acc with { Months = filledMonths };
        }).ToList();
    }
}

public record MonthTotal(Guid MonthId, int Year, int MonthNumber, decimal Total, bool Unpaid = false);
public record AccountFixedExpenseSummary(Guid AccountId, string AccountName, List<MonthTotal> Months);
public record DashboardSummaryDto(List<MonthWithFxRateDto> Months, List<AccountFixedExpenseSummary> FixedExpenses, List<AccountFixedExpenseSummary> Savings, List<AccountFixedExpenseSummary> VariableExpenses, List<AccountFixedExpenseSummary> Investments, decimal KpiCostoMensualArs, decimal KpiCostoMensualUsd);
