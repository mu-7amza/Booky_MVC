using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booky.DataAcess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTotalOrderName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrderTotal",
                table: "OrderHeaders",
                newName: "TotalOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalOrder",
                table: "OrderHeaders",
                newName: "OrderTotal");
        }
    }
}
