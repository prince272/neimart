using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Add_Store_Logo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "StoreLogoId",
                table: "User",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_StoreLogoId",
                table: "User",
                column: "StoreLogoId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Media_StoreLogoId",
                table: "User",
                column: "StoreLogoId",
                principalTable: "Media",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Media_StoreLogoId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_StoreLogoId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "StoreLogoId",
                table: "User");
        }
    }
}
