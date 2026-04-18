using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReMarket.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddItemSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Items",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            // Backfill unique slugs for existing rows before enforcing NOT NULL and unique index.
            migrationBuilder.Sql(
                "UPDATE Items SET Slug = N'item-' + CAST(Id AS nvarchar(20)) WHERE Slug IS NULL OR Slug = N'';");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Items",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_Slug",
                table: "Items",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Items_Slug",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Items");
        }
    }
}
