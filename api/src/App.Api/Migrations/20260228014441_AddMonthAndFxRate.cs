using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace App.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthAndFxRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "months",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    MonthNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_months", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fx_rate_months",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MonthId = table.Column<Guid>(type: "uuid", nullable: false),
                    BaseCurrency = table.Column<string>(type: "text", nullable: false),
                    QuoteCurrency = table.Column<string>(type: "text", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fx_rate_months", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fx_rate_months_months_MonthId",
                        column: x => x.MonthId,
                        principalTable: "months",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "months",
                columns: new[] { "Id", "MonthNumber", "Year" },
                values: new object[,]
                {
                    { new Guid("22222222-0000-0000-0000-000000000001"), 12, 2025 },
                    { new Guid("22222222-0000-0000-0000-000000000002"), 1, 2026 },
                    { new Guid("22222222-0000-0000-0000-000000000003"), 2, 2026 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_fx_rate_months_MonthId",
                table: "fx_rate_months",
                column: "MonthId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fx_rate_months");

            migrationBuilder.DropTable(
                name: "months");
        }
    }
}
