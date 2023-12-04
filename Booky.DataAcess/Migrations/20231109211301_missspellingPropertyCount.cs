using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booky.DataAcess.Migrations
{
    /// <inheritdoc />
    public partial class missspellingPropertyCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "count",
                table: "ShoppingCarts",
                newName: "Count");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Count",
                table: "ShoppingCarts",
                newName: "count");
        }
    }
}
