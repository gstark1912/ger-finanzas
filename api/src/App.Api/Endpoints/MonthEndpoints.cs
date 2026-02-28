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
        group.MapGet("/", async (AppDbContext db, int count = 3) =>
        {
            var months = await db.Months
                .Include(m => m.FxRate)
                .OrderByDescending(m => m.Year).ThenByDescending(m => m.MonthNumber)
                .Take(count)
                .ToListAsync();

            return Results.Ok(months.Select(m => new MonthWithFxRateDto(
                m.Id, m.Year, m.MonthNumber, m.FxRate?.Rate)));
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
