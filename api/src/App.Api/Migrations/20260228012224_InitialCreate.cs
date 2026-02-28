using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace App.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "expense_accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expense_accounts", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "expense_accounts");
        }
    }
}
