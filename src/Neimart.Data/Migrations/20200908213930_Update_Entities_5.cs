using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Value",
                table: "Tag");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Tag",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Tag");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "Tag",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
