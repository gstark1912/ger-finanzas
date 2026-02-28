using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace App.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSavingAccountMonths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "saving_account_months",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SavingAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    MonthId = table.Column<Guid>(type: "uuid", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saving_account_months", x => x.Id);
                    table.ForeignKey(
                        name: "FK_saving_account_months_months_MonthId",
                        column: x => x.MonthId,
                        principalTable: "months",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_saving_account_months_saving_accounts_SavingAccountId",
                        column: x => x.SavingAccountId,
                        principalTable: "saving_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "saving_account_months",
                columns: new[] { "Id", "Balance", "MonthId", "SavingAccountId" },
                values: new object[,]
                {
                    { new Guid("77777777-0000-0000-0000-000000000001"), 20547.16m, new Guid("22222222-0000-0000-0000-000000000003"), new Guid("66666666-0000-0000-0000-000000000001") },
                    { new Guid("77777777-0000-0000-0000-000000000002"), 32027.61m, new Guid("22222222-0000-0000-0000-000000000003"), new Guid("66666666-0000-0000-0000-000000000002") },
                    { new Guid("77777777-0000-0000-0000-000000000003"), 0m, new Guid("22222222-0000-0000-0000-000000000003"), new Guid("66666666-0000-0000-0000-000000000003") },
                    { new Guid("77777777-0000-0000-0000-000000000004"), 12745m, new Guid("22222222-0000-0000-0000-000000000003"), new Guid("66666666-0000-0000-0000-000000000004") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_saving_account_months_MonthId",
                table: "saving_account_months",
                column: "MonthId");

            migrationBuilder.CreateIndex(
                name: "IX_saving_account_months_SavingAccountId_MonthId",
                table: "saving_account_months",
                columns: new[] { "SavingAccountId", "MonthId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "saving_account_months");
        }
    }
}
