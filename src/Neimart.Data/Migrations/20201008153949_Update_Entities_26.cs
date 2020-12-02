using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_26 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Address");

            migrationBuilder.AddColumn<string>(
                name: "StoreCountryCode",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoreCountryName",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorePostalCode",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoreRegionCode",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoreRegionName",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine",
                table: "Address",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "AddressLine",
                table: "Address");

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Address",
                type: "decimal(18, 2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Address",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Address",
                type: "decimal(18, 2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
