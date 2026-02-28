using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSavingAccountCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "saving_accounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "saving_accounts",
                keyColumn: "Id",
                keyValue: new Guid("66666666-0000-0000-0000-000000000001"),
                column: "Currency",
                value: "USD");

            migrationBuilder.UpdateData(
                table: "saving_accounts",
                keyColumn: "Id",
                keyValue: new Guid("66666666-0000-0000-0000-000000000002"),
                column: "Currency",
                value: "USD");

            migrationBuilder.UpdateData(
                table: "saving_accounts",
                keyColumn: "Id",
                keyValue: new Guid("66666666-0000-0000-0000-000000000003"),
                column: "Currency",
                value: "USD");

            migrationBuilder.UpdateData(
                table: "saving_accounts",
                keyColumn: "Id",
                keyValue: new Guid("66666666-0000-0000-0000-000000000004"),
                column: "Currency",
                value: "USD");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "saving_accounts");
        }
    }
}
