using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_27 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThemeMode",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ThemeStyle",
                table: "User");

            migrationBuilder.AddColumn<int>(
                name: "StoreThemeMode",
                table: "User",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StoreThemeStyle",
                table: "User",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoreThemeMode",
                table: "User");

            migrationBuilder.DropColumn(
                name: "StoreThemeStyle",
                table: "User");

            migrationBuilder.AddColumn<int>(
                name: "ThemeMode",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ThemeStyle",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
