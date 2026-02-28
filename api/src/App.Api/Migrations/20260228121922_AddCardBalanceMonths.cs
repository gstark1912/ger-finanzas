using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCardBalanceMonths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "card_balance_months",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpenseAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    OtherExpensesArs = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OtherExpensesUsd = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Paid = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_balance_months", x => x.Id);
                    table.ForeignKey(
                        name: "FK_card_balance_months_expense_accounts_ExpenseAccountId",
                        column: x => x.ExpenseAccountId,
                        principalTable: "expense_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_card_balance_months_ExpenseAccountId_Month_Year",
                table: "card_balance_months",
                columns: new[] { "ExpenseAccountId", "Month", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "card_balance_months");
        }
    }
}
