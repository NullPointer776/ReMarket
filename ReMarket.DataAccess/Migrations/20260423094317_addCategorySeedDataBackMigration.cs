using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ReMarket.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addCategorySeedDataBackMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "IsActive", "Name", "ParentCategoryId", "Slug" },
                values: new object[,]
                {
                    { 6, "Comfortable seating", true, "Sofas", 2, "sofas" },
                    { 7, "Dining and office tables", true, "Tables", 2, "tables" },
                    { 8, "Groceries and drinks", true, "Food & Beverages", null, "food-beverages" },
                    { 9, "Fiction and non-fiction books", true, "Books", null, "books" },
                    { 10, "Toys and board games", true, "Toys & Games", null, "toys-games" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 10);
        }
    }
}
