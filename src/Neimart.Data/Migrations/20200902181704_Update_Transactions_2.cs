using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Transactions_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessUrl",
                table: "Transaction");

            migrationBuilder.AddColumn<string>(
                name: "CallbackUrl",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Transaction",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CallbackUrl",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Transaction");

            migrationBuilder.AddColumn<string>(
                name: "ProcessUrl",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
