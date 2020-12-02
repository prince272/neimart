using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_43 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneIssuer",
                table: "User");

            migrationBuilder.AddColumn<bool>(
                name: "BankSetup",
                table: "User",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MobileIssuer",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobileNumber",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MobileSetup",
                table: "User",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankSetup",
                table: "User");

            migrationBuilder.DropColumn(
                name: "MobileIssuer",
                table: "User");

            migrationBuilder.DropColumn(
                name: "MobileNumber",
                table: "User");

            migrationBuilder.DropColumn(
                name: "MobileSetup",
                table: "User");

            migrationBuilder.AddColumn<string>(
                name: "PhoneIssuer",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
