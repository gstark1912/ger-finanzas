using Microsoft.EntityFrameworkCore;
using App.Api.Data;
using App.Api.Models;

namespace App.Api.Endpoints;

public static class DashboardCalculator
{
    public static async Task<DashboardSummaryDto> Calculate(AppDbContext db, List<Month> months, Currency targetCurrency)
    {
        var monthIds = months.Select(m => m.Id).ToList();

        var entries = await db.FixedExpenseMonthEntries
            .Include(e => e.Definition).ThenInclude(d => d.ExpenseAccount)
            .Where(e => monthIds.Contains(e.MonthId))
            .ToListAsync();

        var fixedExpenses = entries
            .GroupBy(e => e.Definition.ExpenseAccount)
            .Select(accountGroup =>
            {
                var byMonth = months.Select(m =>
                {
                    var rate = m.FxRate?.Rate ?? 0m;
                    var total = accountGroup
                        .Where(e => e.MonthId == m.Id)
                        .Sum(e => Convert(e.Amount, e.Definition.Currency, targetCurrency, rate));
                    return new MonthTotal(m.Id, m.Year, m.MonthNumber, total);
                }).ToList();
                return new AccountFixedExpenseSummary(accountGroup.Key.Id, accountGroup.Key.Name, byMonth);
            }).ToList();

        var ccAccounts = await db.ExpenseAccounts
            .Where(a => a.Type == ExpenseAccountType.CC && a.IsActive).ToListAsync();

        var cardExpenseMonths = await db.CardExpenseMonths
            .Where(e => ccAccounts.Select(a => a.Id).Contains(e.CardInstallment.ExpenseAccountId)
                && months.Select(m => m.Year * 100 + m.MonthNumber).Contains(e.Year * 100 + e.Month))
            .Include(e => e.CardInstallment)
            .ToListAsync();

        var cardBalanceMonths = await db.CardBalanceMonths
            .Where(b => ccAccounts.Select(a => a.Id).Contains(b.ExpenseAccountId)
                && months.Select(m => m.Year * 100 + m.MonthNumber).Contains(b.Year * 100 + b.Month))
            .ToListAsync();

        var nonCcAccounts = await db.ExpenseAccounts
            .Where(a => a.Type != ExpenseAccountType.CC && a.IsActive).ToListAsync();

        var nonCcVariableExpenses = await db.VariableExpenses
            .Where(v => nonCcAccounts.Select(a => a.Id).Contains(v.ExpenseAccountId)
                && months.Select(m => m.Year * 100 + m.MonthNumber).Contains(v.Year * 100 + v.Month))
            .ToListAsync();

        var variableExpenses = ccAccounts.Select(account =>
        {
            var byMonth = months.Select(m =>
            {
                var rate = m.FxRate?.Rate ?? 0m;
                var installmentsTotal = cardExpenseMonths
                    .Where(e => e.CardInstallment.ExpenseAccountId == account.Id && e.Month == m.MonthNumber && e.Year == m.Year)
                    .Sum(e => Convert(e.Total, e.Currency, targetCurrency, rate));
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
        })
        .Concat(nonCcAccounts.Select(account =>
        {
            var byMonth = months.Select(m =>
            {
                var rate = m.FxRate?.Rate ?? 0m;
                var ve = nonCcVariableExpenses.FirstOrDefault(v => v.ExpenseAccountId == account.Id && v.Month == m.MonthNumber && v.Year == m.Year);
                if (ve == null) return new MonthTotal(m.Id, m.Year, m.MonthNumber, 0m);
                return new MonthTotal(m.Id, m.Year, m.MonthNumber, Convert(ve.Total, ve.Currency, targetCurrency, rate));
            }).ToList();
            return new AccountFixedExpenseSummary(account.Id, account.Name, byMonth);
        }))
        .ToList();

        var savingAccountMonths = await db.SavingAccountMonths
            .Include(s => s.SavingAccount)
            .Include(s => s.Month).ThenInclude(m => m.FxRate)
            .Where(s => monthIds.Contains(s.MonthId))
            .ToListAsync();

        var savingTransactions = await db.SavingAccountMonthTransactions
            .Where(t => savingAccountMonths.Select(s => s.Id).Contains(t.SavingAccountMonthId))
            .ToListAsync();

        var savings = savingAccountMonths
            .GroupBy(s => s.SavingAccount)
            .Select(accountGroup =>
            {
                var byMonth = months.Select(m =>
                {
                    var rate = m.FxRate?.Rate ?? 0m;
                    var entry = accountGroup.FirstOrDefault(s => s.MonthId == m.Id);
                    var balance = entry == null ? 0m : Convert(entry.Balance, entry.SavingAccount.Currency, targetCurrency, rate);
                    return new MonthTotal(m.Id, m.Year, m.MonthNumber, balance);
                }).ToList();
                return new AccountFixedExpenseSummary(accountGroup.Key.Id, accountGroup.Key.Name, byMonth);
            }).ToList();

        var investmentMonths = await db.InvestmentAccountMonths
            .Include(i => i.InvestmentAccount)
            .Where(i => months.Select(m => m.Year * 100 + m.MonthNumber).Contains(i.Year * 100 + i.Month))
            .ToListAsync();

        var investments = investmentMonths
            .GroupBy(i => i.InvestmentAccount)
            .Select(accountGroup =>
            {
                var byMonth = months.Select(m =>
                {
                    var rate = m.FxRate?.Rate ?? 0m;
                    var entry = accountGroup.FirstOrDefault(i => i.Month == m.MonthNumber && i.Year == m.Year);
                    var balance = entry == null ? 0m : Convert(entry.Balance, accountGroup.Key.Currency, targetCurrency, rate);
                    return new MonthTotal(m.Id, m.Year, m.MonthNumber, balance);
                }).ToList();
                return new AccountFixedExpenseSummary(accountGroup.Key.Id, accountGroup.Key.Name, byMonth);
            }).ToList();

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

        // Patrimonio USD delta: current month vs previous month
        var orderedMonths = months.OrderBy(m => m.Year).ThenBy(m => m.MonthNumber).ToList();
        decimal PatrimonioUsd(Month m) {
            var rate = m.FxRate?.Rate ?? 1m;
            var caja = savings.Sum(a => a.Months.FirstOrDefault(x => x.MonthId == m.Id)?.Total ?? 0m);
            var inv = investments.Sum(a => a.Months.FirstOrDefault(x => x.MonthId == m.Id)?.Total ?? 0m);
            var total = caja + inv;
            return targetCurrency == Currency.USD ? total : (rate > 0 ? total / rate : 0m);
        }
        var lastMonth = orderedMonths.Last();
        var prevMonth = orderedMonths.Count > 1 ? orderedMonths[^2] : null;
        var kpiPatrimonioUsdDelta = prevMonth is not null ? PatrimonioUsd(lastMonth) - PatrimonioUsd(prevMonth) : 0m;

        // Total ingresos por mes: saving transactions (positivas) + investment income, minus saving expenses + investment expenses
        var totalIngresos = months.Select(m =>
        {
            var rate = m.FxRate?.Rate ?? 1m;
            var samIds = savingAccountMonths.Where(s => s.MonthId == m.Id).Select(s => s.Id).ToHashSet();
            var savingIncome = savingTransactions
                .Where(t => samIds.Contains(t.SavingAccountMonthId) && t.Amount > 0)
                .Sum(t => t.Amount);
            var savingExpenses = savingTransactions
                .Where(t => samIds.Contains(t.SavingAccountMonthId) && t.Amount < 0)
                .Sum(t => t.Amount);
            // SavingAccount is always USD
            var savingNet = Convert(savingIncome + savingExpenses, Currency.USD, targetCurrency, rate);

            var invNet = investmentMonths
                .Where(i => i.Month == m.MonthNumber && i.Year == m.Year)
                .Sum(i => Convert(i.Income - i.Expenses, i.InvestmentAccount.Currency, targetCurrency, rate));

            return new MonthTotal(m.Id, m.Year, m.MonthNumber, savingNet + invNet);
        }).ToList();

        var monthHeaders = months
            .OrderBy(m => m.Year).ThenBy(m => m.MonthNumber)
            .Select(m => new MonthWithFxRateDto(m.Id, m.Year, m.MonthNumber, m.FxRate?.Rate))
            .ToList();

        // Promedio ahorro 6m: average of totalIngresos (in USD) for up to last 6 months
        var savingsInUsd = totalIngresos
            .OrderByDescending(t => t.Year * 100 + t.MonthNumber)
            .Take(6)
            .Select(t =>
            {
                var m = months.FirstOrDefault(x => x.Id == t.MonthId);
                var rate = m?.FxRate?.Rate ?? 1m;
                return targetCurrency == Currency.USD ? t.Total : (rate > 0 ? t.Total / rate : 0m);
            })
            .ToList();
        var kpiPromedioAhorro6m = savingsInUsd.Count > 0 ? savingsInUsd.Average() : 0m;

        return new DashboardSummaryDto(monthHeaders, fixedExpenses, savings, variableExpenses, investments, kpiArs, kpiUsd, kpiPatrimonioUsdDelta, totalIngresos, kpiPromedioAhorro6m);
    }

    private static decimal Convert(decimal amount, Currency from, Currency to, decimal rate)
    {
        if (from == to) return amount;
        if (to == Currency.ARS) return amount * rate;
        return rate > 0 ? amount / rate : 0m;
    }
}
