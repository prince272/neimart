using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Orders_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductSlug",
                table: "OrderItem");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "OrderItem",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "OrderItem");

            migrationBuilder.AddColumn<string>(
                name: "ProductSlug",
                table: "OrderItem",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
