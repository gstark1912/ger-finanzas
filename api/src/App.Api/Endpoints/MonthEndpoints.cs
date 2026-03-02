using Microsoft.EntityFrameworkCore;
using App.Api.Data;
using App.Api.Models;

namespace App.Api.Endpoints;

public static class MonthEndpoints
{
    public static void MapMonthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/months").WithTags("Months");

        // GET last N months with their fx rate
        group.MapGet("/", async (AppDbContext db, int count = 6) =>
        {
            var months = await db.Months
                .Include(m => m.FxRate)
                .OrderByDescending(m => m.Year).ThenByDescending(m => m.MonthNumber)
                .Take(count)
                .ToListAsync();

            return Results.Ok(months.Select(m => new MonthWithFxRateDto(
                m.Id, m.Year, m.MonthNumber, m.FxRate?.Rate)));
        });

        // POST create month with fx rate 1:1
        group.MapPost("/", async (CreateMonthRequest req, AppDbContext db) =>
        {
            var existing = await db.Months.FirstOrDefaultAsync(m => m.Year == req.Year && m.MonthNumber == req.Month);
            if (existing is not null) return Results.Conflict(new { error = "Month already exists" });
            var month = new Month { Id = Guid.NewGuid(), Year = req.Year, MonthNumber = req.Month };
            db.Months.Add(month);
            db.FxRateMonths.Add(new FxRateMonth { Id = Guid.NewGuid(), MonthId = month.Id, BaseCurrency = "USD", QuoteCurrency = "ARS", Rate = 1m });
            await db.SaveChangesAsync();
            return Results.Created($"/api/months/{month.Id}", new MonthWithFxRateDto(month.Id, month.Year, month.MonthNumber, 1m));
        });

        // PUT upsert fx rate for a month
        group.MapPut("/{id:guid}/fx-rate", async (Guid id, UpsertFxRateRequest request, AppDbContext db) =>
        {
            var month = await db.Months.Include(m => m.FxRate).FirstOrDefaultAsync(m => m.Id == id);
            if (month is null) return Results.NotFound();

            if (month.FxRate is null)
            {
                db.FxRateMonths.Add(new FxRateMonth
                {
                    Id = Guid.NewGuid(),
                    MonthId = id,
                    BaseCurrency = "USD",
                    QuoteCurrency = "ARS",
                    Rate = request.Rate
                });
            }
            else
            {
                month.FxRate.Rate = request.Rate;
            }

            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
