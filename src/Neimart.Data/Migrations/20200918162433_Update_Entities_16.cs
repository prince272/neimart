﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Neimart.Data.Migrations
{
    public partial class Update_Entities_16 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "StorePlanUpdated",
                table: "User",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StorePlanUpdatedOn",
                table: "User",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StorePlanUpdated",
                table: "User");

            migrationBuilder.DropColumn(
                name: "StorePlanUpdatedOn",
                table: "User");
        }
    }
}
