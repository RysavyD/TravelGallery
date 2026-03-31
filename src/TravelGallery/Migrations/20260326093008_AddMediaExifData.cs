using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelGallery.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaExifData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CameraModel",
                schema: "travel",
                table: "Media",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTaken",
                schema: "travel",
                table: "Media",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExifSummary",
                schema: "travel",
                table: "Media",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                schema: "travel",
                table: "Media",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                schema: "travel",
                table: "Media",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CameraModel",
                schema: "travel",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "DateTaken",
                schema: "travel",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "ExifSummary",
                schema: "travel",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "Latitude",
                schema: "travel",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "Longitude",
                schema: "travel",
                table: "Media");
        }
    }
}
