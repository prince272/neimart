using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_13 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StorePlan",
                table: "User");

            migrationBuilder.AddColumn<int>(
                name: "StorePlanPeriod",
                table: "User",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StorePlanType",
                table: "User",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StorePlanPeriod",
                table: "User");

            migrationBuilder.DropColumn(
                name: "StorePlanType",
                table: "User");

            migrationBuilder.AddColumn<int>(
                name: "StorePlan",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
