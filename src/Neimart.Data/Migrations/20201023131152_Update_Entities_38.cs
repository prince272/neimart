using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_38 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoreAddress",
                table: "User");

            migrationBuilder.DropColumn(
                name: "StoreCountryCode",
                table: "User");

            migrationBuilder.DropColumn(
                name: "StoreCountryName",
                table: "User");

            migrationBuilder.DropColumn(
                name: "StorePostalCode",
                table: "User");

            migrationBuilder.DropColumn(
                name: "StoreRegionCode",
                table: "User");

            migrationBuilder.DropColumn(
                name: "StoreRegionName",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Address");

            migrationBuilder.AddColumn<string>(
                name: "StorePlace",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorePostal",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoreRegion",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoreStreet",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Postal",
                table: "Address",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StorePlace",
                table: "User");

            migrationBuilder.DropColumn(
                name: "StorePostal",
                table: "User");

            migrationBuilder.DropColumn(
                name: "StoreRegion",
                table: "User");

            migrationBuilder.DropColumn(
                name: "StoreStreet",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Postal",
                table: "Address");

            migrationBuilder.AddColumn<string>(
                name: "StoreAddress",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoreCountryCode",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoreCountryName",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorePostalCode",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoreRegionCode",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoreRegionName",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Address",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
