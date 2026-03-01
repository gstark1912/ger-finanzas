using Microsoft.EntityFrameworkCore;
using App.Api.Data;
using App.Api.Models;

namespace App.Api.Endpoints;

public static class VariableExpenseEndpoints
{
    public static void MapVariableExpenseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/variable-expenses").WithTags("Variable Expenses");

        // GET entries for last N months
        group.MapGet("/", async (AppDbContext db, int count = 3) =>
        {
            var cutoff = await db.Months
                .OrderByDescending(m => m.Year).ThenByDescending(m => m.MonthNumber)
                .Take(count).Select(m => new { m.Year, m.MonthNumber })
                .ToListAsync();

            if (!cutoff.Any()) return Results.Ok(Array.Empty<VariableExpenseDto>());

            var minYear = cutoff.Min(m => m.Year);
            var maxYear = cutoff.Max(m => m.Year);

            var items = await db.VariableExpenses
                .Where(e => e.Year >= minYear && e.Year <= maxYear)
                .ToListAsync();

            var valid = cutoff.Select(m => (m.Year, m.MonthNumber)).ToHashSet();
            var filtered = items.Where(e => valid.Contains((e.Year, e.Month)));

            return Results.Ok(filtered.Select(ToDto));
        });

        // DELETE by id
        group.MapDelete("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var entry = await db.VariableExpenses.FindAsync(id);
            if (entry is null) return Results.NotFound();
            db.VariableExpenses.Remove(entry);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // PUT upsert for a specific account/month/year
        group.MapPut("/", async (Guid expenseAccountId, int month, int year, UpsertVariableExpenseRequest req, AppDbContext db) =>
        {
            var account = await db.ExpenseAccounts.FindAsync(expenseAccountId);
            if (account is null) return Results.BadRequest(new { error = "ExpenseAccount not found" });

            var entry = await db.VariableExpenses
                .FirstOrDefaultAsync(e => e.ExpenseAccountId == expenseAccountId && e.Month == month && e.Year == year);

            if (entry is null)
            {
                entry = new VariableExpense
                {
                    Id = Guid.NewGuid(),
                    ExpenseAccountId = expenseAccountId,
                    Currency = account.Currency,
                    Month = month,
                    Year = year,
                    Total = req.Total
                };
                db.VariableExpenses.Add(entry);
            }
            else
            {
                entry.Total = req.Total;
            }

            await db.SaveChangesAsync();
            return Results.Ok(ToDto(entry));
        });
    }

    private static VariableExpenseDto ToDto(VariableExpense e) =>
        new(e.Id, e.ExpenseAccountId, e.Total, e.Currency.ToString(), e.Month, e.Year);
}
