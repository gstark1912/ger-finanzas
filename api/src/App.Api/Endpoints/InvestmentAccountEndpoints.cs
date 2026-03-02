using Microsoft.EntityFrameworkCore;
using App.Api.Data;
using App.Api.Models;

namespace App.Api.Endpoints;

public static class InvestmentAccountEndpoints
{
    public static void MapInvestmentAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/investment-accounts").WithTags("Investment Accounts");

        group.MapGet("/", async (AppDbContext db) =>
        {
            var items = await db.InvestmentAccounts.OrderBy(a => a.Name).ToListAsync();
            return Results.Ok(items.Select(ToDto));
        });

        group.MapPost("/", async (SaveInvestmentAccountRequest req, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(req.Name)) return Results.BadRequest(new { error = "Name is required" });
            if (!Enum.TryParse<Currency>(req.Currency, true, out var currency)) return Results.BadRequest(new { error = "Currency must be USD or ARS" });

            var account = new InvestmentAccount { Id = Guid.NewGuid(), Name = req.Name.Trim(), Currency = currency, IsActive = true, ExpectedAnnualReturnPct = req.ExpectedAnnualReturnPct, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            db.InvestmentAccounts.Add(account);
            await db.SaveChangesAsync();
            return Results.Created($"/api/investment-accounts/{account.Id}", ToDto(account));
        });

        group.MapPut("/{id:guid}", async (Guid id, SaveInvestmentAccountRequest req, AppDbContext db) =>
        {
            var account = await db.InvestmentAccounts.FindAsync(id);
            if (account is null) return Results.NotFound();
            if (string.IsNullOrWhiteSpace(req.Name)) return Results.BadRequest(new { error = "Name is required" });
            if (!Enum.TryParse<Currency>(req.Currency, true, out var currency)) return Results.BadRequest(new { error = "Currency must be USD or ARS" });

            account.Name = req.Name.Trim();
            account.Currency = currency;
            account.IsActive = req.IsActive;
            account.ExpectedAnnualReturnPct = req.ExpectedAnnualReturnPct;
            account.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ToDto(account));
        });

        group.MapDelete("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var account = await db.InvestmentAccounts.FindAsync(id);
            if (account is null) return Results.NotFound();
            account.IsActive = false;
            account.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // GET months for a range, always including current month per active account
        app.MapGet("/api/investment-account-months", async (AppDbContext db, int month, int year, int count = 3) =>
        {
            var periods = Enumerable.Range(0, count)
                .Select(i => { var d = new DateTime(year, month, 1).AddMonths(-i); return (d.Month, d.Year); })
                .ToList();
            var items = await db.InvestmentAccountMonths
                .Where(m => periods.Select(p => p.Month).Contains(m.Month) && periods.Select(p => p.Year).Contains(m.Year))
                .ToListAsync();
            items = items.Where(m => periods.Any(p => p.Month == m.Month && p.Year == m.Year)).ToList();

            var activeAccountIds = await db.InvestmentAccounts.Where(a => a.IsActive).Select(a => a.Id).ToListAsync();
            var placeholders = activeAccountIds
                .Where(id => !items.Any(m => m.InvestmentAccountId == id && m.Month == month && m.Year == year))
                .Select(id => new InvestmentAccountMonthDto(Guid.Empty, id, month, year, 0, 0, 0, false));

            return Results.Ok(items.Select(ToMonthDto).Concat(placeholders));
        }).WithTags("Investment Accounts");

        // PUT upsert month
        app.MapPut("/api/investment-account-months", async (Guid investmentAccountId, int month, int year, UpsertInvestmentAccountMonthRequest req, AppDbContext db) =>
        {
            var existing = await db.InvestmentAccountMonths
                .FirstOrDefaultAsync(m => m.InvestmentAccountId == investmentAccountId && m.Month == month && m.Year == year);
            if (existing is null)
                db.InvestmentAccountMonths.Add(new InvestmentAccountMonth { Id = Guid.NewGuid(), InvestmentAccountId = investmentAccountId, Month = month, Year = year, Balance = req.Balance, Income = req.Income, Expenses = req.Expenses });
            else
            { existing.Balance = req.Balance; existing.Income = req.Income; existing.Expenses = req.Expenses; }
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithTags("Investment Accounts");
    }

    private static InvestmentAccountDto ToDto(InvestmentAccount a) =>
        new(a.Id, a.Name, a.Currency.ToString(), a.IsActive, a.ExpectedAnnualReturnPct, a.CreatedAt, a.UpdatedAt);

    private static InvestmentAccountMonthDto ToMonthDto(InvestmentAccountMonth m) =>
        new(m.Id, m.InvestmentAccountId, m.Month, m.Year, m.Balance, m.Income, m.Expenses, true);
}
