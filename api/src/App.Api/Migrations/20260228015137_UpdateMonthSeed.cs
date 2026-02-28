using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace App.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMonthSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "months",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "months",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000002"));

            migrationBuilder.InsertData(
                table: "fx_rate_months",
                columns: new[] { "Id", "BaseCurrency", "MonthId", "QuoteCurrency", "Rate" },
                values: new object[] { new Guid("33333333-0000-0000-0000-000000000001"), "USD", new Guid("22222222-0000-0000-0000-000000000003"), "ARS", 1450m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "fx_rate_months",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000001"));

            migrationBuilder.InsertData(
                table: "months",
                columns: new[] { "Id", "MonthNumber", "Year" },
                values: new object[,]
                {
                    { new Guid("22222222-0000-0000-0000-000000000001"), 12, 2025 },
                    { new Guid("22222222-0000-0000-0000-000000000002"), 1, 2026 }
                });
        }
    }
}
