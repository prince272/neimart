using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_24 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidDuring",
                table: "Order");

            migrationBuilder.AddColumn<bool>(
                name: "StoreSetup",
                table: "User",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoreSetup",
                table: "User");

            migrationBuilder.AddColumn<int>(
                name: "PaidDuring",
                table: "Order",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
