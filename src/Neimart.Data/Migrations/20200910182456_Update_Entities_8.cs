using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_8 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "StoreDocumentId",
                table: "User",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_StoreDocumentId",
                table: "User",
                column: "StoreDocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Media_StoreDocumentId",
                table: "User",
                column: "StoreDocumentId",
                principalTable: "Media",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Media_StoreDocumentId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_StoreDocumentId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "StoreDocumentId",
                table: "User");
        }
    }
}
