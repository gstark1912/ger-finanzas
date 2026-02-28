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

            var ccAccounts = await db.ExpenseAccounts
                .Where(a => a.Type == ExpenseAccountType.CC && a.IsActive)
                .ToListAsync();

            var cardExpenseMonths = await db.CardExpenseMonths
                .Where(e => ccAccounts.Select(a => a.Id).Contains(e.CardInstallment.ExpenseAccountId)
                    && months.Select(m => m.Year * 100 + m.MonthNumber).Contains(e.Year * 100 + e.Month))
                .Include(e => e.CardInstallment)
                .ToListAsync();

            var cardBalanceMonths = await db.CardBalanceMonths
                .Where(b => ccAccounts.Select(a => a.Id).Contains(b.ExpenseAccountId)
                    && months.Select(m => m.Year * 100 + m.MonthNumber).Contains(b.Year * 100 + b.Month))
                .ToListAsync();

            var variableExpenses = ccAccounts.Select(account =>
            {
                var byMonth = months.Select(m =>
                {
                    var rate = m.FxRate?.Rate ?? 0m;
                    var installmentsTotal = cardExpenseMonths
                        .Where(e => e.CardInstallment.ExpenseAccountId == account.Id && e.Month == m.MonthNumber && e.Year == m.Year)
                        .Sum(e =>
                        {
                            if (e.Currency == targetCurrency) return e.Total;
                            if (targetCurrency == Currency.ARS) return e.Total * rate;
                            return rate > 0 ? e.Total / rate : 0m;
                        });
                    var balance = cardBalanceMonths.FirstOrDefault(b => b.ExpenseAccountId == account.Id && b.Month == m.MonthNumber && b.Year == m.Year);
                    decimal balanceTotal = 0m;
                    if (balance != null)
                    {
                        balanceTotal = targetCurrency == Currency.ARS
                            ? balance.OtherExpensesArs + balance.OtherExpensesUsd * rate
                            : (rate > 0 ? balance.OtherExpensesArs / rate : 0m) + balance.OtherExpensesUsd;
                    }
                    return new MonthTotal(m.Id, m.Year, m.MonthNumber, installmentsTotal + balanceTotal, balance != null && !balance.Paid);
                }).ToList();
                return new AccountFixedExpenseSummary(account.Id, account.Name, byMonth);
            }).ToList();

            var savingAccountMonths = await db.SavingAccountMonths
                .Include(s => s.SavingAccount)
                .Include(s => s.Month).ThenInclude(m => m.FxRate)
                .Where(s => monthIds.Contains(s.MonthId))
                .ToListAsync();

            var savings = savingAccountMonths
                .GroupBy(s => s.SavingAccount)
                .Select(accountGroup =>
                {
                    var byMonth = months.Select(m =>
                    {
                        var rate = m.FxRate?.Rate ?? 0m;
                        var entry = accountGroup.FirstOrDefault(s => s.MonthId == m.Id);
                        var balance = entry == null ? 0m : entry.SavingAccount.Currency == targetCurrency
                            ? entry.Balance
                            : targetCurrency == Currency.ARS
                                ? entry.Balance * rate
                                : rate > 0 ? entry.Balance / rate : 0m;
                        return new MonthTotal(m.Id, m.Year, m.MonthNumber, balance);
                    }).ToList();
                    return new AccountFixedExpenseSummary(accountGroup.Key.Id, accountGroup.Key.Name, byMonth);
                })
                .ToList();

            var now = DateTime.UtcNow;
            var currentMonth = months.FirstOrDefault(m => m.Year == now.Year && m.MonthNumber == now.Month)
                ?? months.OrderByDescending(m => m.Year).ThenByDescending(m => m.MonthNumber).First();
            var currentRate = currentMonth.FxRate?.Rate ?? 0m;

            decimal SumForMonth(IEnumerable<AccountFixedExpenseSummary> groups, Currency toCurrency)
            {
                return groups.Sum(acc =>
                {
                    var mt = acc.Months.FirstOrDefault(x => x.MonthId == currentMonth.Id);
                    if (mt == null) return 0m;
                    if (targetCurrency == toCurrency) return mt.Total;
                    if (toCurrency == Currency.ARS) return mt.Total * currentRate;
                    return currentRate > 0 ? mt.Total / currentRate : 0m;
                });
            }

            var kpiArs = SumForMonth(fixedExpenses, Currency.ARS) + SumForMonth(variableExpenses, Currency.ARS);
            var kpiUsd = SumForMonth(fixedExpenses, Currency.USD) + SumForMonth(variableExpenses, Currency.USD);

            var monthHeaders = months
                .OrderBy(m => m.Year).ThenBy(m => m.MonthNumber)
                .Select(m => new MonthWithFxRateDto(m.Id, m.Year, m.MonthNumber, m.FxRate?.Rate))
                .ToList();

            return Results.Ok(new DashboardSummaryDto(monthHeaders, fixedExpenses, savings, variableExpenses, kpiArs, kpiUsd));
        })
        .WithTags("Dashboard");
    }
}

public record MonthTotal(Guid MonthId, int Year, int MonthNumber, decimal Total, bool Unpaid = false);
public record AccountFixedExpenseSummary(Guid AccountId, string AccountName, List<MonthTotal> Months);
public record DashboardSummaryDto(List<MonthWithFxRateDto> Months, List<AccountFixedExpenseSummary> FixedExpenses, List<AccountFixedExpenseSummary> Savings, List<AccountFixedExpenseSummary> VariableExpenses, decimal KpiCostoMensualArs, decimal KpiCostoMensualUsd);
