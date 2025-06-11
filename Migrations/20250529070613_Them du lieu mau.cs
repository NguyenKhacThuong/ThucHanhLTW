using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buoi6.Migrations
{
    /// <inheritdoc />
    public partial class Themdulieumau : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: ["Id", "Name"],
                values: [1, "May bay"]
            );


            migrationBuilder.InsertData(
                table: "Products",
                columns: ["Id", "Name", "Price", "Description", "ImageUrl", "CategoryId"],
                values: [1, "boeing 666", 333333.0, "Sieu to vl", null, 1]
            );

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
