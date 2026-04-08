using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuoiKy.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ExpressShipping",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GiftWrap",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpressShipping",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GiftWrap",
                table: "Orders");
        }
    }
}
