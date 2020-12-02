using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FulfillmentNote",
                table: "User");

            migrationBuilder.AddColumn<string>(
                name: "ReturnNote",
                table: "User",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnNote",
                table: "User");

            migrationBuilder.AddColumn<string>(
                name: "FulfillmentNote",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
