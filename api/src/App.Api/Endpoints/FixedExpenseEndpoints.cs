using Microsoft.EntityFrameworkCore;
using App.Api.Data;
using App.Api.Models;

namespace App.Api.Endpoints;

public static class FixedExpenseEndpoints
{
    public static void MapFixedExpenseEndpoints(this IEndpointRouteBuilder app)
    {
        var defs = app.MapGroup("/api/fixed-expense-definitions").WithTags("Fixed Expenses");
        var entries = app.MapGroup("/api/fixed-expense-entries").WithTags("Fixed Expenses");

        // GET all active definitions
        defs.MapGet("/", async (AppDbContext db) =>
        {
            var items = await db.FixedExpenseDefinitions
                .Where(d => d.IsActive)
                .OrderBy(d => d.ExpenseAccountId).ThenBy(d => d.Name)
                .ToListAsync();
            return Results.Ok(items.Select(ToDto));
        });

        // POST create definition
        defs.MapPost("/", async (CreateFixedExpenseDefinitionRequest req, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.BadRequest(new { error = "Name is required" });

            if (!Enum.TryParse<Currency>(req.Currency, true, out var currency))
                return Results.BadRequest(new { error = "Currency must be USD or ARS" });

            var account = await db.ExpenseAccounts.FindAsync(req.ExpenseAccountId);
            if (account is null)
                return Results.BadRequest(new { error = "ExpenseAccount not found" });

            var def = new FixedExpenseDefinition
            {
                Id = Guid.NewGuid(),
                Name = req.Name.Trim(),
                ExpenseAccountId = req.ExpenseAccountId,
                Currency = currency,
                IsActive = true,
                ExpireDay = req.ExpireDay,
                CreatedAt = DateTime.UtcNow
            };

            db.FixedExpenseDefinitions.Add(def);
            await db.SaveChangesAsync();
            return Results.Created($"/api/fixed-expense-definitions/{def.Id}", ToDto(def));
        });

        // DELETE (soft) definition
        defs.MapDelete("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var def = await db.FixedExpenseDefinitions.FindAsync(id);
            if (def is null) return Results.NotFound();
            def.IsActive = false;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // PUT update definition
        defs.MapPut("/{id:guid}", async (Guid id, CreateFixedExpenseDefinitionRequest req, AppDbContext db) =>
        {
            var def = await db.FixedExpenseDefinitions.FindAsync(id);
            if (def is null) return Results.NotFound();

            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.BadRequest(new { error = "Name is required" });

            if (!Enum.TryParse<Currency>(req.Currency, true, out var currency))
                return Results.BadRequest(new { error = "Currency must be USD or ARS" });

            var account = await db.ExpenseAccounts.FindAsync(req.ExpenseAccountId);
            if (account is null) return Results.BadRequest(new { error = "ExpenseAccount not found" });

            def.Name = req.Name.Trim();
            def.ExpenseAccountId = req.ExpenseAccountId;
            def.Currency = currency;
            def.ExpireDay = req.ExpireDay;
            await db.SaveChangesAsync();
            return Results.Ok(ToDto(def));
        });

        // GET entries for last 3 months (used to build the grid)
        entries.MapGet("/", async (AppDbContext db) =>
        {
            var monthIds = await db.Months
                .OrderByDescending(m => m.Year).ThenByDescending(m => m.MonthNumber)
                .Take(3).Select(m => m.Id).ToListAsync();

            var items = await db.FixedExpenseMonthEntries
                .Where(e => monthIds.Contains(e.MonthId))
                .ToListAsync();

            return Results.Ok(items.Select(ToEntryDto));
        });

        // POST pay (create entry)
        entries.MapPost("/", async (Guid definitionId, Guid monthId, PayFixedExpenseRequest req, AppDbContext db) =>
        {
            var exists = await db.FixedExpenseMonthEntries
                .AnyAsync(e => e.FixedExpenseDefinitionId == definitionId && e.MonthId == monthId);
            if (exists)
                return Results.Conflict(new { error = "Entry already exists for this month" });

            var entry = new FixedExpenseMonthEntry
            {
                Id = Guid.NewGuid(),
                FixedExpenseDefinitionId = definitionId,
                MonthId = monthId,
                Amount = req.Amount,
                PaidAt = DateTime.UtcNow
            };

            db.FixedExpenseMonthEntries.Add(entry);
            await db.SaveChangesAsync();
            return Results.Created($"/api/fixed-expense-entries/{entry.Id}", ToEntryDto(entry));
        });

        // PUT edit entry (only allowed for latest month — enforced in frontend)
        entries.MapPut("/{id:guid}", async (Guid id, PayFixedExpenseRequest req, AppDbContext db) =>
        {
            var entry = await db.FixedExpenseMonthEntries.FindAsync(id);
            if (entry is null) return Results.NotFound();
            entry.Amount = req.Amount;
            entry.PaidAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ToEntryDto(entry));
        });

        // DELETE entry
        entries.MapDelete("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var entry = await db.FixedExpenseMonthEntries.FindAsync(id);
            if (entry is null) return Results.NotFound();
            db.FixedExpenseMonthEntries.Remove(entry);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }

    private static FixedExpenseDefinitionDto ToDto(FixedExpenseDefinition d) =>
        new(d.Id, d.Name, d.ExpenseAccountId, d.Currency.ToString(), d.IsActive, d.ExpireDay, d.CreatedAt);

    private static FixedExpenseMonthEntryDto ToEntryDto(FixedExpenseMonthEntry e) =>
        new(e.Id, e.FixedExpenseDefinitionId, e.MonthId, e.Amount, e.PaidAt);
}
