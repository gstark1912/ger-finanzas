using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace App.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedFixedExpenseEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "fixed_expense_month_entries",
                columns: new[] { "Id", "Amount", "FixedExpenseDefinitionId", "MonthId", "PaidAt" },
                values: new object[,]
                {
                    { new Guid("55555555-0000-0000-0000-000000000001"), 235930m, new Guid("44444444-0000-0000-0000-000000000001"), new Guid("22222222-0000-0000-0000-000000000003"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-0000-0000-0000-000000000002"), 437000m, new Guid("44444444-0000-0000-0000-000000000004"), new Guid("22222222-0000-0000-0000-000000000003"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-0000-0000-0000-000000000003"), 49000m, new Guid("44444444-0000-0000-0000-000000000008"), new Guid("22222222-0000-0000-0000-000000000003"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-0000-0000-0000-000000000004"), 95000m, new Guid("44444444-0000-0000-0000-000000000007"), new Guid("22222222-0000-0000-0000-000000000003"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-0000-0000-0000-000000000005"), 1373000m, new Guid("44444444-0000-0000-0000-000000000006"), new Guid("22222222-0000-0000-0000-000000000003"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-0000-0000-0000-000000000006"), 92000m, new Guid("44444444-0000-0000-0000-000000000005"), new Guid("22222222-0000-0000-0000-000000000003"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-0000-0000-0000-000000000007"), 78285m, new Guid("44444444-0000-0000-0000-000000000009"), new Guid("22222222-0000-0000-0000-000000000003"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-0000-0000-0000-000000000008"), 155358.33m, new Guid("44444444-0000-0000-0000-000000000010"), new Guid("22222222-0000-0000-0000-000000000003"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-0000-0000-0000-000000000009"), 8628.35m, new Guid("44444444-0000-0000-0000-000000000011"), new Guid("22222222-0000-0000-0000-000000000003"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-0000-0000-0000-000000000010"), 0m, new Guid("44444444-0000-0000-0000-000000000012"), new Guid("22222222-0000-0000-0000-000000000003"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-0000-0000-0000-000000000011"), 0m, new Guid("44444444-0000-0000-0000-000000000013"), new Guid("22222222-0000-0000-0000-000000000003"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-0000-0000-0000-000000000012"), 0m, new Guid("44444444-0000-0000-0000-000000000014"), new Guid("22222222-0000-0000-0000-000000000003"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-0000-0000-0000-000000000013"), 0m, new Guid("44444444-0000-0000-0000-000000000015"), new Guid("22222222-0000-0000-0000-000000000003"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "fixed_expense_month_entries",
                keyColumn: "Id",
                keyValue: new Guid("55555555-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_month_entries",
                keyColumn: "Id",
                keyValue: new Guid("55555555-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_month_entries",
                keyColumn: "Id",
                keyValue: new Guid("55555555-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_month_entries",
                keyColumn: "Id",
                keyValue: new Guid("55555555-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_month_entries",
                keyColumn: "Id",
                keyValue: new Guid("55555555-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_month_entries",
                keyColumn: "Id",
                keyValue: new Guid("55555555-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_month_entries",
                keyColumn: "Id",
                keyValue: new Guid("55555555-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_month_entries",
                keyColumn: "Id",
                keyValue: new Guid("55555555-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_month_entries",
                keyColumn: "Id",
                keyValue: new Guid("55555555-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_month_entries",
                keyColumn: "Id",
                keyValue: new Guid("55555555-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_month_entries",
                keyColumn: "Id",
                keyValue: new Guid("55555555-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_month_entries",
                keyColumn: "Id",
                keyValue: new Guid("55555555-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_month_entries",
                keyColumn: "Id",
                keyValue: new Guid("55555555-0000-0000-0000-000000000013"));
        }
    }
}
