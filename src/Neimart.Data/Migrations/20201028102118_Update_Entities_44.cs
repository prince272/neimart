using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_44 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Transaction");

            migrationBuilder.AddColumn<string>(
                name: "AuthorizationCode",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "Transaction",
                type: "decimal(18, 2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TransactionCode",
                table: "Transaction",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorizationCode",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "TransactionCode",
                table: "Transaction");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
