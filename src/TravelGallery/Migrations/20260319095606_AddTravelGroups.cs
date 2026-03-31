using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelGallery.Migrations
{
    /// <inheritdoc />
    public partial class AddTravelGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TravelGroups",
                schema: "travel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TravelGroupMembers",
                schema: "travel",
                columns: table => new
                {
                    GroupsId = table.Column<int>(type: "int", nullable: false),
                    MembersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelGroupMembers", x => new { x.GroupsId, x.MembersId });
                    table.ForeignKey(
                        name: "FK_TravelGroupMembers_AspNetUsers_MembersId",
                        column: x => x.MembersId,
                        principalSchema: "travel",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TravelGroupMembers_TravelGroups_GroupsId",
                        column: x => x.GroupsId,
                        principalSchema: "travel",
                        principalTable: "TravelGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TravelGroupTrips",
                schema: "travel",
                columns: table => new
                {
                    GroupsId = table.Column<int>(type: "int", nullable: false),
                    TripsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelGroupTrips", x => new { x.GroupsId, x.TripsId });
                    table.ForeignKey(
                        name: "FK_TravelGroupTrips_TravelGroups_GroupsId",
                        column: x => x.GroupsId,
                        principalSchema: "travel",
                        principalTable: "TravelGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TravelGroupTrips_Trips_TripsId",
                        column: x => x.TripsId,
                        principalSchema: "travel",
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TravelGroupMembers_MembersId",
                schema: "travel",
                table: "TravelGroupMembers",
                column: "MembersId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelGroupTrips_TripsId",
                schema: "travel",
                table: "TravelGroupTrips",
                column: "TripsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TravelGroupMembers",
                schema: "travel");

            migrationBuilder.DropTable(
                name: "TravelGroupTrips",
                schema: "travel");

            migrationBuilder.DropTable(
                name: "TravelGroups",
                schema: "travel");
        }
    }
}
