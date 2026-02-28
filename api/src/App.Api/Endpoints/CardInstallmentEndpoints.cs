using Microsoft.EntityFrameworkCore;
using App.Api.Data;
using App.Api.Models;

namespace App.Api.Endpoints;

public static class CardInstallmentEndpoints
{
    public static void MapCardInstallmentEndpoints(this IEndpointRouteBuilder app)
    {
        // GET summary: grouped by CC account, filtered by month range and currency
        app.MapGet("/api/card-installments/summary", async (AppDbContext db, string currency = "ARS") =>
        {
            if (!Enum.TryParse<Currency>(currency, true, out var targetCurrency))
                return Results.BadRequest(new { error = "currency must be ARS or USD" });

            var dbMonths = await db.Months
                .Include(m => m.FxRate)
                .OrderByDescending(m => m.Year).ThenByDescending(m => m.MonthNumber)
                .Take(3)
                .ToListAsync();

            var lastMonth = dbMonths.First();
            var lastRate = lastMonth.FxRate?.Rate ?? 0m;

            // Build 6 month slots: 3 real + 3 fabricated
            var allMonths = dbMonths
                .OrderBy(m => m.Year).ThenBy(m => m.MonthNumber)
                .Select(m => (m.Year, Month: m.MonthNumber, Rate: m.FxRate?.Rate ?? 0m, IsReal: true))
                .ToList();

            for (int i = 1; i <= 3; i++)
            {
                var d = new DateTime(lastMonth.Year, lastMonth.MonthNumber, 1).AddMonths(i);
                allMonths.Add((d.Year, d.Month, lastRate, false));
            }

            decimal Convert(decimal amount, Currency from, decimal rate)
            {
                if (from == targetCurrency) return amount;
                if (targetCurrency == Currency.ARS) return amount * rate;
                return rate > 0 ? amount / rate : 0m;
            }

            var ccAccounts = await db.ExpenseAccounts
                .Where(a => a.Type == ExpenseAccountType.CC && a.IsActive)
                .OrderBy(a => a.Name)
                .ToListAsync();

            var monthKeys = allMonths.Select(m => m.Year * 100 + m.Month).ToList();

            var installments = await db.CardInstallments
                .Include(c => c.CardExpenseMonths)
                .Where(c => c.Active)
                .ToListAsync();

            var balances = await db.CardBalanceMonths
                .Where(b => ccAccounts.Select(a => a.Id).Contains(b.ExpenseAccountId))
                .ToListAsync();

            var monthHeaders = allMonths.Select(m => new { m.Year, m.Month, m.Rate, m.IsReal }).ToList();

            var groups = ccAccounts.Select(account =>
            {
                var accountInstallments = installments
                    .Where(c => c.ExpenseAccountId == account.Id)
                    .Select(c => new
                    {
                        c.Id,
                        c.Description,
                        c.Installments,
                        Months = allMonths.Select(mh =>
                        {
                            var cem = c.CardExpenseMonths.FirstOrDefault(e => e.Year == mh.Year && e.Month == mh.Month);
                            return new
                            {
                                mh.Year,
                                mh.Month,
                                Total = cem != null ? Convert(cem.Total, cem.Currency, mh.Rate) : 0m,
                                cem?.Paid,
                                CemId = cem?.Id,
                                InstallmentNumber = cem?.Installment
                            };
                        }).ToList()
                    }).ToList();

                var accountBalances = allMonths.Select(mh =>
                {
                    var b = balances.FirstOrDefault(x => x.ExpenseAccountId == account.Id && x.Year == mh.Year && x.Month == mh.Month);
                    var otherTotal = b != null ? Convert(b.OtherExpensesArs, Currency.ARS, mh.Rate) + Convert(b.OtherExpensesUsd, Currency.USD, mh.Rate) : 0m;
                    return new { mh.Year, mh.Month, OtherTotal = otherTotal, b?.Paid, BalanceId = b?.Id, b?.OtherExpensesArs, b?.OtherExpensesUsd };
                }).ToList();

                return new { AccountId = account.Id, AccountName = account.Name, AccountCurrency = account.Currency.ToString(), Installments = accountInstallments, Balances = accountBalances };
            }).ToList();

            return Results.Ok(new { MonthHeaders = monthHeaders, Groups = groups });
        })
        .WithTags("Cards");

        // POST create installment + generate CardExpenseMonths
        app.MapPost("/api/card-installments", async (AppDbContext db, CreateCardInstallmentRequest req) =>
        {
            if (!Enum.TryParse<Currency>(req.Currency, true, out var currency))
                return Results.BadRequest(new { error = "currency must be ARS or USD" });

            var account = await db.ExpenseAccounts.FindAsync(req.ExpenseAccountId);
            if (account == null || account.Type != ExpenseAccountType.CC)
                return Results.BadRequest(new { error = "ExpenseAccount must be of type CC" });

            var installment = new CardInstallment
            {
                Id = Guid.NewGuid(),
                ExpenseAccountId = req.ExpenseAccountId,
                Description = req.Description,
                Total = req.Total,
                Currency = currency,
                Installments = Math.Max(1, req.Installments),
                Date = req.Date,
                StartingMonth = req.StartingMonth
            };

            var installmentAmount = Math.Round(req.Total / installment.Installments, 2);
            var startDate = DateTime.UtcNow.AddMonths(req.StartingMonth);

            for (int i = 0; i < installment.Installments; i++)
            {
                var d = startDate.AddMonths(i);
                installment.CardExpenseMonths.Add(new CardExpenseMonth
                {
                    Id = Guid.NewGuid(),
                    CardInstallmentId = installment.Id,
                    Total = installmentAmount,
                    Currency = currency,
                    Installment = i + 1,
                    Month = d.Month,
                    Year = d.Year,
                    Paid = false
                });
            }

            db.CardInstallments.Add(installment);
            await db.SaveChangesAsync();

            return Results.Created($"/api/card-installments/{installment.Id}", installment.Id);
        })
        .WithTags("Cards");

        // PUT upsert CardBalanceMonth
        app.MapPut("/api/card-balance-months/{expenseAccountId}/{year}/{month}", async (AppDbContext db, Guid expenseAccountId, int year, int month, UpsertCardBalanceMonthRequest req) =>
        {
            var existing = await db.CardBalanceMonths
                .FirstOrDefaultAsync(b => b.ExpenseAccountId == expenseAccountId && b.Year == year && b.Month == month);
            if (existing == null)
            {
                db.CardBalanceMonths.Add(new CardBalanceMonth
                {
                    Id = Guid.NewGuid(),
                    ExpenseAccountId = expenseAccountId,
                    Month = month,
                    Year = year,
                    OtherExpensesArs = req.OtherExpensesArs,
                    OtherExpensesUsd = req.OtherExpensesUsd,
                    Paid = req.Paid
                });
            }
            else
            {
                existing.OtherExpensesArs = req.OtherExpensesArs;
                existing.OtherExpensesUsd = req.OtherExpensesUsd;
                existing.Paid = req.Paid;
            }
            await db.SaveChangesAsync();

            // Sync CardExpenseMonths paid status for this account+month
            var cems = await db.CardInstallments
                .Where(c => c.ExpenseAccountId == expenseAccountId && c.Active)
                .SelectMany(c => c.CardExpenseMonths)
                .Where(e => e.Year == year && e.Month == month)
                .ToListAsync();
            foreach (var cem in cems) cem.Paid = req.Paid;
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithTags("Cards");

        // PATCH toggle paid
        app.MapPatch("/api/card-expense-months/{id}/paid", async (AppDbContext db, Guid id, bool paid) =>
        {
            var cem = await db.CardExpenseMonths.FindAsync(id);
            if (cem == null) return Results.NotFound();
            cem.Paid = paid;
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithTags("Cards");

        // DELETE installment (soft: active = false)
        app.MapDelete("/api/card-installments/{id}", async (AppDbContext db, Guid id) =>
        {
            var installment = await db.CardInstallments.FindAsync(id);
            if (installment == null) return Results.NotFound();
            db.CardInstallments.Remove(installment);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithTags("Cards");
    }
}
