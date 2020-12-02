using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_40 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoreDelivery",
                table: "User");

            migrationBuilder.AddColumn<bool>(
                name: "StoreDeliveryRequired",
                table: "User",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoreDeliveryRequired",
                table: "User");

            migrationBuilder.AddColumn<bool>(
                name: "StoreDelivery",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
