using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_42 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankIssuer",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneIssuer",
                table: "User",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankIssuer",
                table: "User");

            migrationBuilder.DropColumn(
                name: "PhoneIssuer",
                table: "User");
        }
    }
}
