using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_35 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gateway",
                table: "Transaction");

            migrationBuilder.AddColumn<int>(
                name: "Processor",
                table: "Transaction",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Processor",
                table: "Transaction");

            migrationBuilder.AddColumn<int>(
                name: "Gateway",
                table: "Transaction",
                type: "int",
                nullable: true);
        }
    }
}
