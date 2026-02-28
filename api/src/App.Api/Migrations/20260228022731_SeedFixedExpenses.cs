using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace App.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedFixedExpenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "fixed_expense_definitions",
                columns: new[] { "Id", "CreatedAt", "Currency", "ExpenseAccountId", "ExpireDay", "IsActive", "Name" },
                values: new object[,]
                {
                    { new Guid("44444444-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ARS", new Guid("11111111-0000-0000-0000-000000000004"), 20, true, "Osde" },
                    { new Guid("44444444-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ARS", new Guid("11111111-0000-0000-0000-000000000004"), 10, true, "Arba" },
                    { new Guid("44444444-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ARS", new Guid("11111111-0000-0000-0000-000000000004"), 15, true, "Contador" },
                    { new Guid("44444444-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ARS", new Guid("11111111-0000-0000-0000-000000000004"), 20, true, "Impuestos" },
                    { new Guid("44444444-0000-0000-0000-000000000005"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ARS", new Guid("11111111-0000-0000-0000-000000000004"), 22, true, "Personal" },
                    { new Guid("44444444-0000-0000-0000-000000000006"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ARS", new Guid("11111111-0000-0000-0000-000000000001"), 10, true, "Alquiler" },
                    { new Guid("44444444-0000-0000-0000-000000000007"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ARS", new Guid("11111111-0000-0000-0000-000000000001"), 8, true, "Tennis" },
                    { new Guid("44444444-0000-0000-0000-000000000008"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ARS", new Guid("11111111-0000-0000-0000-000000000001"), 10, true, "Gimnasio" },
                    { new Guid("44444444-0000-0000-0000-000000000009"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ARS", new Guid("11111111-0000-0000-0000-000000000002"), null, true, "Seguro Moto" },
                    { new Guid("44444444-0000-0000-0000-000000000010"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ARS", new Guid("11111111-0000-0000-0000-000000000002"), null, true, "Seguro Auto" },
                    { new Guid("44444444-0000-0000-0000-000000000011"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ARS", new Guid("11111111-0000-0000-0000-000000000002"), null, true, "Hbo" },
                    { new Guid("44444444-0000-0000-0000-000000000012"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "USD", new Guid("11111111-0000-0000-0000-000000000002"), null, true, "Youtube" },
                    { new Guid("44444444-0000-0000-0000-000000000013"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "USD", new Guid("11111111-0000-0000-0000-000000000002"), null, true, "Google Drive" },
                    { new Guid("44444444-0000-0000-0000-000000000014"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "USD", new Guid("11111111-0000-0000-0000-000000000002"), null, true, "Spotify" },
                    { new Guid("44444444-0000-0000-0000-000000000015"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "USD", new Guid("11111111-0000-0000-0000-000000000002"), null, true, "Netflix" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "fixed_expense_definitions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_definitions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_definitions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_definitions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_definitions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_definitions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_definitions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_definitions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_definitions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_definitions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_definitions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_definitions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_definitions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_definitions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "fixed_expense_definitions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-0000-0000-000000000015"));
        }
    }
}
