using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_37 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressLine",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "CountryName",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "RegionCode",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "RegionName",
                table: "Address");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Address",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Place",
                table: "Address",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Address",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Address",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "Place",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "Address");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine",
                table: "Address",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Address",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryName",
                table: "Address",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Address",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionCode",
                table: "Address",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionName",
                table: "Address",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
