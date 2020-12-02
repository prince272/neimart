using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentProcess",
                table: "Order");

            migrationBuilder.AddColumn<int>(
                name: "PaidDuring",
                table: "Order",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidDuring",
                table: "Order");

            migrationBuilder.AddColumn<int>(
                name: "PaymentProcess",
                table: "Order",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
