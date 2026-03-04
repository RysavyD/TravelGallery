using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelGallery.Migrations
{
    /// <inheritdoc />
    public partial class UseSchemaTravel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "travel");

            migrationBuilder.RenameTable(
                name: "Trips",
                newName: "Trips",
                newSchema: "travel");

            migrationBuilder.RenameTable(
                name: "Media",
                newName: "Media",
                newSchema: "travel");

            migrationBuilder.RenameTable(
                name: "Comments",
                newName: "Comments",
                newSchema: "travel");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                newName: "AspNetUserTokens",
                newSchema: "travel");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "AspNetUsers",
                newSchema: "travel");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                newName: "AspNetUserRoles",
                newSchema: "travel");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                newName: "AspNetUserLogins",
                newSchema: "travel");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                newName: "AspNetUserClaims",
                newSchema: "travel");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                newName: "AspNetRoles",
                newSchema: "travel");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                newName: "AspNetRoleClaims",
                newSchema: "travel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Trips",
                schema: "travel",
                newName: "Trips");

            migrationBuilder.RenameTable(
                name: "Media",
                schema: "travel",
                newName: "Media");

            migrationBuilder.RenameTable(
                name: "Comments",
                schema: "travel",
                newName: "Comments");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                schema: "travel",
                newName: "AspNetUserTokens");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                schema: "travel",
                newName: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                schema: "travel",
                newName: "AspNetUserRoles");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                schema: "travel",
                newName: "AspNetUserLogins");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                schema: "travel",
                newName: "AspNetUserClaims");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                schema: "travel",
                newName: "AspNetRoles");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                schema: "travel",
                newName: "AspNetRoleClaims");
        }
    }
}
