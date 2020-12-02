using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryRequired",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "DeliveryRequired",
                table: "OrderItem");

            migrationBuilder.AddColumn<bool>(
                name: "DeliveryRequired",
                table: "Order",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryRequired",
                table: "Order");

            migrationBuilder.AddColumn<bool>(
                name: "DeliveryRequired",
                table: "Product",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DeliveryRequired",
                table: "OrderItem",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
