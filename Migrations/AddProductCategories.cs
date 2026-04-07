using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuoiKy.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Category",
                table: "Products",
                newName: "CategoryId");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.Sql(@"
SET IDENTITY_INSERT [Categories] ON;
INSERT INTO [Categories] ([Id], [Name]) VALUES (1, N'Phone');
INSERT INTO [Categories] ([Id], [Name]) VALUES (2, N'Laptop');
INSERT INTO [Categories] ([Id], [Name]) VALUES (3, N'Keyboard');
INSERT INTO [Categories] ([Id], [Name]) VALUES (4, N'Mouse');
INSERT INTO [Categories] ([Id], [Name]) VALUES (5, N'Headphone');
SET IDENTITY_INSERT [Categories] OFF;

UPDATE [Products]
SET [CategoryId] = [CategoryId] + 1
WHERE [CategoryId] BETWEEN 0 AND 4;
");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.Sql(@"
UPDATE [Products]
SET [CategoryId] = [CategoryId] - 1
WHERE [CategoryId] BETWEEN 1 AND 5;
");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Products",
                newName: "Category");
        }
    }
}
