using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_36 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnUrl",
                table: "Transaction");

            migrationBuilder.AddColumn<string>(
                name: "RedirectUrl",
                table: "Transaction",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RedirectUrl",
                table: "Transaction");

            migrationBuilder.AddColumn<string>(
                name: "ReturnUrl",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
