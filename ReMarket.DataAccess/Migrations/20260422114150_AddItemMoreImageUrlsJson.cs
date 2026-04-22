using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReMarket.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddItemMoreImageUrlsJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MoreImageUrlsJson",
                table: "Items",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MoreImageUrlsJson",
                table: "Items");
        }
    }
}
