using Microsoft.EntityFrameworkCore;
using App.Api.Data;
using App.Api.Models;

namespace App.Api.Endpoints;

public static class ExpenseAccountEndpoints
{
    public static void MapExpenseAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/expense-accounts").WithTags("Expense Accounts");

        group.MapGet("/", async (AppDbContext db, bool? activeOnly) =>
        {
            var query = db.ExpenseAccounts.AsQueryable();
            if (activeOnly == true)
                query = query.Where(a => a.IsActive);

            var accounts = await query.OrderBy(a => a.Name).ToListAsync();
            return Results.Ok(accounts.Select(ToDto));
        });

        group.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var account = await db.ExpenseAccounts.FindAsync(id);
            return account is null ? Results.NotFound() : Results.Ok(ToDto(account));
        });

        group.MapPost("/", async (CreateExpenseAccountRequest request, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { error = "Name is required" });

            if (!Enum.TryParse<ExpenseAccountType>(request.Type, true, out var type))
                return Results.BadRequest(new { error = "Invalid account type" });

            if (!Enum.TryParse<Currency>(request.Currency, true, out var currency))
                return Results.BadRequest(new { error = "Currency must be USD or ARS" });

            var account = new ExpenseAccount
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Type = type,
                Currency = currency,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.ExpenseAccounts.Add(account);
            await db.SaveChangesAsync();

            return Results.Created($"/api/expense-accounts/{account.Id}", ToDto(account));
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateExpenseAccountRequest request, AppDbContext db) =>
        {
            var account = await db.ExpenseAccounts.FindAsync(id);
            if (account is null)
                return Results.NotFound();

            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { error = "Name is required" });

            if (!Enum.TryParse<ExpenseAccountType>(request.Type, true, out var type))
                return Results.BadRequest(new { error = "Invalid account type" });

            if (!Enum.TryParse<Currency>(request.Currency, true, out var currency))
                return Results.BadRequest(new { error = "Currency must be USD or ARS" });

            account.Name = request.Name.Trim();
            account.Type = type;
            account.Currency = currency;
            account.IsActive = request.IsActive;
            account.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return Results.Ok(ToDto(account));
        });

        group.MapDelete("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var account = await db.ExpenseAccounts.FindAsync(id);
            if (account is null)
                return Results.NotFound();

            account.IsActive = false;
            account.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }

    private static ExpenseAccountDto ToDto(ExpenseAccount account) =>
        new(account.Id, account.Name, account.Type.ToString(), account.Currency.ToString(), 
            account.IsActive, account.CreatedAt, account.UpdatedAt);
}
