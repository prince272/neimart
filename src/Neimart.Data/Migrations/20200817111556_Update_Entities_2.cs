using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnNote",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "User");

            migrationBuilder.AddColumn<string>(
                name: "ReturnsNote",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewsNote",
                table: "User",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnsNote",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ReviewsNote",
                table: "User");

            migrationBuilder.AddColumn<string>(
                name: "ReturnNote",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
