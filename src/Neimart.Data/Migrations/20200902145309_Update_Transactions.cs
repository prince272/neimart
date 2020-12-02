using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Transactions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_User_MemeberId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_MemeberId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "MemeberId",
                table: "Transaction");

            migrationBuilder.AlterColumn<int>(
                name: "Gateway",
                table: "Transaction",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckoutUrl",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Live",
                table: "Transaction",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "MemberId",
                table: "Transaction",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessUrl",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnUrl",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Transaction",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "Transaction",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_MemberId",
                table: "Transaction",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_User_MemberId",
                table: "Transaction",
                column: "MemberId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_User_MemberId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_MemberId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "CheckoutUrl",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Live",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "ProcessUrl",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "ReturnUrl",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "Transaction");

            migrationBuilder.AlterColumn<string>(
                name: "Gateway",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<long>(
                name: "MemeberId",
                table: "Transaction",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_MemeberId",
                table: "Transaction",
                column: "MemeberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_User_MemeberId",
                table: "Transaction",
                column: "MemeberId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
