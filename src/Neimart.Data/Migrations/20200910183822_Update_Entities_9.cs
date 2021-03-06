﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_9 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoreCategories",
                table: "User");

            migrationBuilder.AddColumn<int>(
                name: "StoreCategory",
                table: "User",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoreCategory",
                table: "User");

            migrationBuilder.AddColumn<int>(
                name: "StoreCategories",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
