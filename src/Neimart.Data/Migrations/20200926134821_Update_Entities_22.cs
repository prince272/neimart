using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_22 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountIssuer",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "Transaction");

            migrationBuilder.AddColumn<string>(
                name: "AccountEmail",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountName",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Transaction",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountEmail",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "AccountName",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Transaction");

            migrationBuilder.AddColumn<string>(
                name: "AccountIssuer",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
