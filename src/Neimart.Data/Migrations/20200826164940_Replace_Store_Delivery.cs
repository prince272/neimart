using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Replace_Store_Delivery : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoreDeliveryRequired",
                table: "User");

            migrationBuilder.AddColumn<bool>(
                name: "StoreDelivery",
                table: "User",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoreDelivery",
                table: "User");

            migrationBuilder.AddColumn<bool>(
                name: "StoreDeliveryRequired",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
