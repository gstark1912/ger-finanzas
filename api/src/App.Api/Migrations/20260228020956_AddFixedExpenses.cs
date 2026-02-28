using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFixedExpenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fixed_expense_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ExpenseAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ExpireDay = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fixed_expense_definitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fixed_expense_definitions_expense_accounts_ExpenseAccountId",
                        column: x => x.ExpenseAccountId,
                        principalTable: "expense_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fixed_expense_month_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FixedExpenseDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    MonthId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fixed_expense_month_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fixed_expense_month_entries_fixed_expense_definitions_Fixed~",
                        column: x => x.FixedExpenseDefinitionId,
                        principalTable: "fixed_expense_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_fixed_expense_month_entries_months_MonthId",
                        column: x => x.MonthId,
                        principalTable: "months",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fixed_expense_definitions_ExpenseAccountId",
                table: "fixed_expense_definitions",
                column: "ExpenseAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_fixed_expense_month_entries_FixedExpenseDefinitionId_MonthId",
                table: "fixed_expense_month_entries",
                columns: new[] { "FixedExpenseDefinitionId", "MonthId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fixed_expense_month_entries_MonthId",
                table: "fixed_expense_month_entries",
                column: "MonthId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fixed_expense_month_entries");

            migrationBuilder.DropTable(
                name: "fixed_expense_definitions");
        }
    }
}
