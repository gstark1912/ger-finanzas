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

            decimal kpiArs, kpiUsd, kpiPatrimonioUsdDelta, kpiPromedioAhorro6m;
            List<MonthTotal> totalIngresos;
            var currentSnap = snapshots.FirstOrDefault(s => s.Year == currentMonth.Year && s.MonthNumber == currentMonth.MonthNumber);
            if (currentSnap != null)
            {
                var snapSummary = JsonSerializer.Deserialize<DashboardSummaryDto>(currentSnap.SummaryJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                var r = currentSnap.FxRate;
                // Snapshot stored in USD, convert KPIs to targetCurrency
                kpiArs = targetCurrency == Currency.ARS ? snapSummary.KpiCostoMensualUsd * r : snapSummary.KpiCostoMensualUsd;
                kpiUsd = snapSummary.KpiCostoMensualUsd;
                kpiPatrimonioUsdDelta = snapSummary.KpiPatrimonioUsdDelta;
                totalIngresos = snapSummary.TotalIngresos.Select(t => t with { Total = targetCurrency == Currency.ARS ? t.Total * r : t.Total }).ToList();
                kpiPromedioAhorro6m = snapSummary.KpiPromedioAhorro6m;
            }
            else
            {
                kpiArs = liveSummary?.KpiCostoMensualArs ?? 0m;
                kpiUsd = liveSummary?.KpiCostoMensualUsd ?? 0m;
                kpiPatrimonioUsdDelta = liveSummary?.KpiPatrimonioUsdDelta ?? 0m;
                totalIngresos = liveSummary?.TotalIngresos ?? [];
                kpiPromedioAhorro6m = liveSummary?.KpiPromedioAhorro6m ?? 0m;
            }

            // Merge totalIngresos from snapshots for non-current months
            foreach (var snap in snapshots.Where(s => !(s.Year == currentMonth.Year && s.MonthNumber == currentMonth.MonthNumber)))
            {
                var snapSummary = JsonSerializer.Deserialize<DashboardSummaryDto>(snap.SummaryJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                var snapMonth = months.First(m => m.Year == snap.Year && m.MonthNumber == snap.MonthNumber);
                var snapEntry = snapSummary.TotalIngresos.FirstOrDefault(t => t.Year == snap.Year && t.MonthNumber == snap.MonthNumber);
                if (snapEntry != null)
                {
                    var converted = targetCurrency == Currency.ARS ? snapEntry.Total * snap.FxRate : snapEntry.Total;
                    totalIngresos = totalIngresos.Where(t => t.MonthId != snapMonth.Id)
                        .Append(snapEntry with { MonthId = snapMonth.Id, Total = converted }).ToList();
                }
            }

            // Recalculate KpiPromedioAhorro6m using all merged totalIngresos (snapshots + live)
            var allSnapshots6m = await db.MonthlySnapshots
                .OrderByDescending(s => s.Year * 100 + s.MonthNumber)
                .Take(6)
                .ToListAsync();
            var savingsForAvg = new List<decimal>();
            // Add live month ahorro if not closed
            var liveAhorro = liveSummary?.TotalIngresos.FirstOrDefault(t => t.MonthId == currentMonth.Id);
            if (liveAhorro != null)
            {
                var liveRate = currentMonth.FxRate?.Rate ?? 1m;
                savingsForAvg.Add(targetCurrency == Currency.USD ? liveAhorro.Total : (liveRate > 0 ? liveAhorro.Total / liveRate : 0m));
            }
            foreach (var s in allSnapshots6m)
            {
                var sd = JsonSerializer.Deserialize<DashboardSummaryDto>(s.SummaryJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                var entry = sd.TotalIngresos.FirstOrDefault(t => t.Year == s.Year && t.MonthNumber == s.MonthNumber);
                if (entry != null) savingsForAvg.Add(sd.KpiPromedioAhorro6m == 0 ? entry.Total : entry.Total); // always USD in snapshot
                if (savingsForAvg.Count >= 6) break;
            }
            kpiPromedioAhorro6m = savingsForAvg.Count > 0 ? savingsForAvg.Average() : 0m;

            // Recalculate KpiPatrimonioUsdDelta: compare current month patrimonio vs previous snapshot
            decimal PatrimonioUsdFromSummary(DashboardSummaryDto s, int y, int mn)
            {
                var rate = allSnapshots6m.FirstOrDefault(x => x.Year == y && x.MonthNumber == mn)?.FxRate ?? 1m;
                var caja = s.Savings.Sum(a => a.Months.FirstOrDefault(t => t.Year == y && t.MonthNumber == mn)?.Total ?? 0m);
                var inv = s.Investments.Sum(a => a.Months.FirstOrDefault(t => t.Year == y && t.MonthNumber == mn)?.Total ?? 0m);
                var total = caja + inv;
                return targetCurrency == Currency.USD ? total : (rate > 0 ? total / rate : 0m);
            }
            var prevSnapshot = await db.MonthlySnapshots
                .Where(s => s.Year * 100 + s.MonthNumber < currentMonth.Year * 100 + currentMonth.MonthNumber)
                .OrderByDescending(s => s.Year * 100 + s.MonthNumber)
                .FirstOrDefaultAsync();
            if (prevSnapshot != null)
            {
                var prevSd = JsonSerializer.Deserialize<DashboardSummaryDto>(prevSnapshot.SummaryJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                var prevPatrimonio = PatrimonioUsdFromSummary(prevSd, prevSnapshot.Year, prevSnapshot.MonthNumber);
                decimal currentPatrimonio;
                if (currentSnap != null)
                {
                    var curSd = JsonSerializer.Deserialize<DashboardSummaryDto>(currentSnap.SummaryJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                    currentPatrimonio = PatrimonioUsdFromSummary(curSd, currentMonth.Year, currentMonth.MonthNumber);
                }
                else
                {
                    var liveRate = currentMonth.FxRate?.Rate ?? 1m;
                    var cajaCurrent = mergedSavings.Sum(a => a.Months.FirstOrDefault(t => t.MonthId == currentMonth.Id)?.Total ?? 0m);
                    var invCurrent = mergedInvestments.Sum(a => a.Months.FirstOrDefault(t => t.MonthId == currentMonth.Id)?.Total ?? 0m);
                    var total = cajaCurrent + invCurrent;
                    currentPatrimonio = targetCurrency == Currency.USD ? total : (liveRate > 0 ? total / liveRate : 0m);
                }
                kpiPatrimonioUsdDelta = currentPatrimonio - prevPatrimonio;
            }

            return Results.Ok(new DashboardSummaryDto(allMonthHeaders, mergedFixedExpenses, mergedSavings, mergedVariableExpenses, mergedInvestments, kpiArs, kpiUsd, kpiPatrimonioUsdDelta, totalIngresos, kpiPromedioAhorro6m));
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

        if (liveSummary != null)
            foreach (var acc in selector(liveSummary))
                result[acc.AccountId] = acc;

        foreach (var snap in snapshots)
        {
            var snapSummary = JsonSerializer.Deserialize<DashboardSummaryDto>(snap.SummaryJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            var month = months.First(m => m.Year == snap.Year && m.MonthNumber == snap.MonthNumber);
            var rate = snap.FxRate;

            foreach (var acc in selector(snapSummary))
            {
                var snapMonthTotal = acc.Months.FirstOrDefault(mt => mt.Year == snap.Year && mt.MonthNumber == snap.MonthNumber);
                if (snapMonthTotal == null) continue;

                // Snapshot is stored in USD — convert to targetCurrency
                var converted = targetCurrency == Currency.ARS ? snapMonthTotal.Total * rate : snapMonthTotal.Total;
                var remapped = snapMonthTotal with { MonthId = month.Id, Total = converted };

                if (result.TryGetValue(acc.AccountId, out var existing))
                {
                    var updatedMonths = existing.Months.Where(mt => mt.MonthId != month.Id).Append(remapped).ToList();
                    result[acc.AccountId] = existing with { Months = updatedMonths };
                }
                else
                    result[acc.AccountId] = acc with { Months = [remapped] };
            }
        }

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
public record DashboardSummaryDto(List<MonthWithFxRateDto> Months, List<AccountFixedExpenseSummary> FixedExpenses, List<AccountFixedExpenseSummary> Savings, List<AccountFixedExpenseSummary> VariableExpenses, List<AccountFixedExpenseSummary> Investments, decimal KpiCostoMensualArs, decimal KpiCostoMensualUsd, decimal KpiPatrimonioUsdDelta, List<MonthTotal> TotalIngresos, decimal KpiPromedioAhorro6m);
