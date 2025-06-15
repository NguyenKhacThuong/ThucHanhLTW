using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buoi6.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductAndProductImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Url",
                table: "ProductImages",
                newName: "ImageUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "ProductImages",
                newName: "Url");
        }
    }
}
