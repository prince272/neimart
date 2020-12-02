using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_17 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Fee",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Live",
                table: "Transaction");

            migrationBuilder.AddColumn<string>(
                name: "AccountIssuer",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mode",
                table: "Transaction",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountIssuer",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Mode",
                table: "Transaction");

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "Transaction",
                type: "decimal(18, 2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Fee",
                table: "Transaction",
                type: "decimal(18, 2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Live",
                table: "Transaction",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
