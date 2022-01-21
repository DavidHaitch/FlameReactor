using Microsoft.EntityFrameworkCore.Migrations;

namespace FlameReactor.DB.Migrations
{
    public partial class RenameTweetRecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tweet");

            migrationBuilder.CreateTable(
                name: "TweetRecord",
                columns: table => new
                {
                    ID = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Faves = table.Column<int>(type: "INTEGER", nullable: false),
                    Retweets = table.Column<int>(type: "INTEGER", nullable: false),
                    FlameID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TweetRecord", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TweetRecord_Flames_FlameID",
                        column: x => x.FlameID,
                        principalTable: "Flames",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TweetRecord_FlameID",
                table: "TweetRecord",
                column: "FlameID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TweetRecord");

            migrationBuilder.CreateTable(
                name: "Tweet",
                columns: table => new
                {
                    ID = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Faves = table.Column<int>(type: "INTEGER", nullable: false),
                    FlameID = table.Column<int>(type: "INTEGER", nullable: true),
                    Retweets = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tweet", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tweet_Flames_FlameID",
                        column: x => x.FlameID,
                        principalTable: "Flames",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tweet_FlameID",
                table: "Tweet",
                column: "FlameID");
        }
    }
}
