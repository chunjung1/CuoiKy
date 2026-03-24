using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuoiKy.Migrations
{
    /// <inheritdoc />
    public partial class FixMissingUserColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Manually add the columns that EF thinks already exist but are missing in the DB
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResetToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetTokenExpiry",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Email", table: "Users");
            migrationBuilder.DropColumn(name: "ResetToken", table: "Users");
            migrationBuilder.DropColumn(name: "ResetTokenExpiry", table: "Users");
        }
    }
}
