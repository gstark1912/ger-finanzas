using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace App.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "expense_accounts",
                keyColumn: "Id",
                keyValue: new Guid("23e385a1-0100-48e2-9e28-39927c49a665"));

            migrationBuilder.DeleteData(
                table: "expense_accounts",
                keyColumn: "Id",
                keyValue: new Guid("46b8e90f-0a8c-4a4d-b21c-20475bdd0c6a"));

            migrationBuilder.DeleteData(
                table: "expense_accounts",
                keyColumn: "Id",
                keyValue: new Guid("e61211ae-aa2d-4426-8e31-a4c44deffb93"));

            migrationBuilder.InsertData(
                table: "expense_accounts",
                columns: new[] { "Id", "CreatedAt", "Currency", "IsActive", "Name", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ARS", true, "CASH $", "Cash", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ARS", true, "Tarjeta Visa", "CC", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ARS", true, "Tarjeta Amex", "CC", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ARS", true, "Santander $", "Bank", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000005"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "USD", true, "Citi USD", "Bank", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "expense_accounts",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "expense_accounts",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "expense_accounts",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "expense_accounts",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "expense_accounts",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000005"));

            migrationBuilder.InsertData(
                table: "expense_accounts",
                columns: new[] { "Id", "CreatedAt", "Currency", "IsActive", "Name", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("23e385a1-0100-48e2-9e28-39927c49a665"), new DateTime(2026, 2, 28, 1, 22, 23, 885, DateTimeKind.Utc).AddTicks(1818), "USD", true, "Wise USD", "Bank", new DateTime(2026, 2, 28, 1, 22, 23, 885, DateTimeKind.Utc).AddTicks(1821) },
                    { new Guid("46b8e90f-0a8c-4a4d-b21c-20475bdd0c6a"), new DateTime(2026, 2, 28, 1, 22, 23, 885, DateTimeKind.Utc).AddTicks(1823), "USD", true, "Citi USD", "Bank", new DateTime(2026, 2, 28, 1, 22, 23, 885, DateTimeKind.Utc).AddTicks(1823) },
                    { new Guid("e61211ae-aa2d-4426-8e31-a4c44deffb93"), new DateTime(2026, 2, 28, 1, 22, 23, 885, DateTimeKind.Utc).AddTicks(1824), "USD", true, "Cash USD", "Cash", new DateTime(2026, 2, 28, 1, 22, 23, 885, DateTimeKind.Utc).AddTicks(1825) }
                });
        }
    }
}
