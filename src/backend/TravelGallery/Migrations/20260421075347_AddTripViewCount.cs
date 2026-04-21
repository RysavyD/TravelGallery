using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelGallery.Migrations
{
    /// <inheritdoc />
    public partial class AddTripViewCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                schema: "travel",
                table: "Trips",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewCount",
                schema: "travel",
                table: "Trips");
        }
    }
}
