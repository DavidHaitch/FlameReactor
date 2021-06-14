using Microsoft.EntityFrameworkCore.Migrations;

namespace FlameReactor.DB.Migrations
{
    public partial class Breedings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BirthID",
                table: "Flames",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Breeding",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstParentID = table.Column<int>(type: "INTEGER", nullable: false),
                    SecondParentID = table.Column<int>(type: "INTEGER", nullable: false),
                    ChildID = table.Column<int>(type: "INTEGER", nullable: false),
                    FlameID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Breeding", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Breeding_Flames_ChildID",
                        column: x => x.ChildID,
                        principalTable: "Flames",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Breeding_Flames_FirstParentID",
                        column: x => x.FirstParentID,
                        principalTable: "Flames",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Breeding_Flames_FlameID",
                        column: x => x.FlameID,
                        principalTable: "Flames",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Breeding_Flames_SecondParentID",
                        column: x => x.SecondParentID,
                        principalTable: "Flames",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Flames_BirthID",
                table: "Flames",
                column: "BirthID");

            migrationBuilder.CreateIndex(
                name: "IX_Breeding_ChildID",
                table: "Breeding",
                column: "ChildID");

            migrationBuilder.CreateIndex(
                name: "IX_Breeding_FirstParentID",
                table: "Breeding",
                column: "FirstParentID");

            migrationBuilder.CreateIndex(
                name: "IX_Breeding_FlameID",
                table: "Breeding",
                column: "FlameID");

            migrationBuilder.CreateIndex(
                name: "IX_Breeding_SecondParentID",
                table: "Breeding",
                column: "SecondParentID");

            migrationBuilder.AddForeignKey(
                name: "FK_Flames_Breeding_BirthID",
                table: "Flames",
                column: "BirthID",
                principalTable: "Breeding",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flames_Breeding_BirthID",
                table: "Flames");

            migrationBuilder.DropTable(
                name: "Breeding");

            migrationBuilder.DropIndex(
                name: "IX_Flames_BirthID",
                table: "Flames");

            migrationBuilder.DropColumn(
                name: "BirthID",
                table: "Flames");
        }
    }
}
