using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCCDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaCierre",
                table: "expense_accounts",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaVencimiento",
                table: "expense_accounts",
                type: "date",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "expense_accounts",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000001"),
                columns: new[] { "FechaCierre", "FechaVencimiento" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "expense_accounts",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000002"),
                columns: new[] { "FechaCierre", "FechaVencimiento" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "expense_accounts",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000003"),
                columns: new[] { "FechaCierre", "FechaVencimiento" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "expense_accounts",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000004"),
                columns: new[] { "FechaCierre", "FechaVencimiento" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "expense_accounts",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000005"),
                columns: new[] { "FechaCierre", "FechaVencimiento" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaCierre",
                table: "expense_accounts");

            migrationBuilder.DropColumn(
                name: "FechaVencimiento",
                table: "expense_accounts");
        }
    }
}
