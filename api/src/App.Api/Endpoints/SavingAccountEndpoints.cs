using Microsoft.EntityFrameworkCore;
using App.Api.Data;
using App.Api.Models;

namespace App.Api.Endpoints;

public static class SavingAccountEndpoints
{
    public static void MapSavingAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/saving-accounts").WithTags("Saving Accounts");

        group.MapGet("/", async (AppDbContext db) =>
        {
            var items = await db.SavingAccounts.OrderBy(a => a.Name).ToListAsync();
            return Results.Ok(items.Select(ToDto));
        });

        group.MapPost("/", async (SaveSavingAccountRequest req, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(req.Name)) return Results.BadRequest(new { error = "Name is required" });
            if (!Enum.TryParse<SavingAccountType>(req.Type, true, out var type)) return Results.BadRequest(new { error = "Type must be Bank or Cash" });
            if (!Enum.TryParse<Currency>(req.Currency, true, out var currency)) return Results.BadRequest(new { error = "Currency must be USD or ARS" });

            var account = new SavingAccount { Id = Guid.NewGuid(), Name = req.Name.Trim(), Type = type, Currency = currency, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            db.SavingAccounts.Add(account);
            await db.SaveChangesAsync();
            return Results.Created($"/api/saving-accounts/{account.Id}", ToDto(account));
        });

        group.MapPut("/{id:guid}", async (Guid id, SaveSavingAccountRequest req, AppDbContext db) =>
        {
            var account = await db.SavingAccounts.FindAsync(id);
            if (account is null) return Results.NotFound();
            if (string.IsNullOrWhiteSpace(req.Name)) return Results.BadRequest(new { error = "Name is required" });
            if (!Enum.TryParse<SavingAccountType>(req.Type, true, out var type)) return Results.BadRequest(new { error = "Type must be Bank or Cash" });
            if (!Enum.TryParse<Currency>(req.Currency, true, out var currency)) return Results.BadRequest(new { error = "Currency must be USD or ARS" });

            account.Name = req.Name.Trim();
            account.Type = type;
            account.Currency = currency;
            account.IsActive = req.IsActive;
            account.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ToDto(account));
        });

        group.MapDelete("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var account = await db.SavingAccounts.FindAsync(id);
            if (account is null) return Results.NotFound();
            account.IsActive = false;
            account.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // GET balances for last N months
        app.MapGet("/api/saving-account-months", async (AppDbContext db, int count = 3) =>
        {
            var monthIds = await db.Months
                .OrderByDescending(m => m.Year).ThenByDescending(m => m.MonthNumber)
                .Take(count).Select(m => m.Id).ToListAsync();
            var items = await db.SavingAccountMonths
                .Where(s => monthIds.Contains(s.MonthId))
                .ToListAsync();
            return Results.Ok(items.Select(s => new SavingAccountMonthDto(s.Id, s.SavingAccountId, s.MonthId, s.Balance)));
        }).WithTags("Saving Accounts");

        // PUT upsert balance
        app.MapPut("/api/saving-account-months", async (Guid savingAccountId, Guid monthId, UpsertSavingAccountMonthRequest req, AppDbContext db) =>
        {
            var existing = await db.SavingAccountMonths
                .FirstOrDefaultAsync(s => s.SavingAccountId == savingAccountId && s.MonthId == monthId);
            if (existing is null)
            {
                db.SavingAccountMonths.Add(new SavingAccountMonth { Id = Guid.NewGuid(), SavingAccountId = savingAccountId, MonthId = monthId, Balance = req.Balance });
            }
            else
            {
                existing.Balance = req.Balance;
            }
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithTags("Saving Accounts");

        // GET transactions for last N months
        app.MapGet("/api/saving-account-transactions", async (AppDbContext db, int count = 3) =>
        {
            var monthIds = await db.Months
                .OrderByDescending(m => m.Year).ThenByDescending(m => m.MonthNumber)
                .Take(count).Select(m => m.Id).ToListAsync();
            var samIds = await db.SavingAccountMonths
                .Where(s => monthIds.Contains(s.MonthId)).Select(s => s.Id).ToListAsync();
            var txs = await db.SavingAccountMonthTransactions
                .Where(t => samIds.Contains(t.SavingAccountMonthId))
                .OrderBy(t => t.Date)
                .ToListAsync();
            return Results.Ok(txs.Select(ToTxDto));
        }).WithTags("Saving Accounts");

        // POST transaction
        app.MapPost("/api/saving-account-transactions", async (Guid savingAccountMonthId, CreateTransactionRequest req, AppDbContext db) =>
        {
            var sam = await db.SavingAccountMonths.FindAsync(savingAccountMonthId);
            if (sam is null) return Results.NotFound();
            var tx = new SavingAccountMonthTransaction
            {
                Id = Guid.NewGuid(),
                SavingAccountMonthId = savingAccountMonthId,
                Amount = req.Amount,
                Date = req.Date,
                Description = req.Description
            };
            db.SavingAccountMonthTransactions.Add(tx);
            sam.Balance += req.Amount > 0 ? Math.Abs(req.Amount) : -Math.Abs(req.Amount);
            await db.SaveChangesAsync();
            return Results.Created($"/api/saving-account-transactions/{tx.Id}", ToTxDto(tx));
        }).WithTags("Saving Accounts");

        // DELETE transaction
        app.MapDelete("/api/saving-account-transactions/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var tx = await db.SavingAccountMonthTransactions.FindAsync(id);
            if (tx is null) return Results.NotFound();
            db.SavingAccountMonthTransactions.Remove(tx);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithTags("Saving Accounts");
        // DELETE all transactions of a sam by type
        app.MapDelete("/api/saving-account-transactions", async (Guid savingAccountMonthId, bool income, AppDbContext db) =>
        {
            var txs = await db.SavingAccountMonthTransactions
                .Where(t => t.SavingAccountMonthId == savingAccountMonthId && (income ? t.Amount > 0 : t.Amount < 0))
                .ToListAsync();
            var delta = txs.Sum(t => t.Amount);
            db.SavingAccountMonthTransactions.RemoveRange(txs);
            var sam = await db.SavingAccountMonths.FindAsync(savingAccountMonthId);
            if (sam is not null) sam.Balance -= delta;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithTags("Saving Accounts");
    }

    private static SavingAccountMonthTransactionDto ToTxDto(SavingAccountMonthTransaction t) =>
        new(t.Id, t.SavingAccountMonthId, t.Amount, t.Date, t.Description);

    private static SavingAccountDto ToDto(SavingAccount a) =>
        new(a.Id, a.Name, a.Type.ToString(), a.Currency.ToString(), a.IsActive, a.CreatedAt, a.UpdatedAt);
}
