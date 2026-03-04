using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Api.Migrations
{
    /// <inheritdoc />
    public partial class ClearAccountMonthData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM saving_account_month_transactions;");
            migrationBuilder.Sql("DELETE FROM saving_account_months;");
            migrationBuilder.Sql("DELETE FROM investment_account_months;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data deletion is irreversible
        }
    }
}
