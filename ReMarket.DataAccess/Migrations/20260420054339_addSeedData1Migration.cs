using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ReMarket.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addSeedData1Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "IconImagePath", "IsActive", "Name", "ParentCategoryId", "Slug" },
                values: new object[,]
                {
                    { 1, "Gadgets and devices", null, true, "Electronics", null, "electronics" },
                    { 2, "Home and office furniture", null, true, "Furniture", null, "furniture" },
                    { 3, "Apparel and accessories", null, true, "Clothing", null, "clothing" },
                    { 4, "Smartphones and accessories", null, true, "Mobile Phones", 1, "mobile-phones" },
                    { 5, "Notebooks and accessories", null, true, "Laptops", 1, "laptops" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
