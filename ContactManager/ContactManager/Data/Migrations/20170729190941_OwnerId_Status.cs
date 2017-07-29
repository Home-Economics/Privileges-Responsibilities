using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactManager.Data.Migrations
{
    public partial class OwnerId_Status : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Contact",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Contact",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Contact");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Contact");
        }
    }
}
