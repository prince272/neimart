using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_12 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ImageId",
                table: "Category",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Category_ImageId",
                table: "Category",
                column: "ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Category_Media_ImageId",
                table: "Category",
                column: "ImageId",
                principalTable: "Media",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Category_Media_ImageId",
                table: "Category");

            migrationBuilder.DropIndex(
                name: "IX_Category_ImageId",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "Category");
        }
    }
}
