using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StorePayments",
                table: "User");

            migrationBuilder.AddColumn<int>(
                name: "StorePayment",
                table: "User",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StorePayment",
                table: "User");

            migrationBuilder.AddColumn<int>(
                name: "StorePayments",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
