using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FlameReactor.DB.Migrations
{
    public partial class TheRestOfTheDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccessEvent",
                columns: table => new
                {
                    Timestamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    IPAddress = table.Column<string>(type: "TEXT", nullable: false),
                    UserAgent = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessEvent", x => new { x.Timestamp, x.IPAddress });
                });

            migrationBuilder.CreateTable(
                name: "InteractionEvent",
                columns: table => new
                {
                    Timestamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    IPAddress = table.Column<string>(type: "TEXT", nullable: false),
                    InteractionType = table.Column<string>(type: "TEXT", nullable: true),
                    Details = table.Column<string>(type: "TEXT", nullable: true),
                    FlameId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractionEvent", x => new { x.Timestamp, x.IPAddress });
                    table.ForeignKey(
                        name: "FK_InteractionEvent_Flames_FlameId",
                        column: x => x.FlameId,
                        principalTable: "Flames",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vote",
                columns: table => new
                {
                    IPAddress = table.Column<string>(type: "TEXT", nullable: false),
                    FlameId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vote", x => new { x.IPAddress, x.FlameId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_InteractionEvent_FlameId",
                table: "InteractionEvent",
                column: "FlameId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessEvent");

            migrationBuilder.DropTable(
                name: "InteractionEvent");

            migrationBuilder.DropTable(
                name: "Vote");
        }
    }
}
