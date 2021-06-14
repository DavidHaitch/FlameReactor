using Microsoft.EntityFrameworkCore.Migrations;

namespace FlameReactor.DB.Migrations
{
    public partial class BreedingsAbstract : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Breeding_Flames_ChildID",
                table: "Breeding");

            migrationBuilder.DropForeignKey(
                name: "FK_Breeding_Flames_FirstParentID",
                table: "Breeding");

            migrationBuilder.DropForeignKey(
                name: "FK_Breeding_Flames_FlameID",
                table: "Breeding");

            migrationBuilder.DropForeignKey(
                name: "FK_Breeding_Flames_SecondParentID",
                table: "Breeding");

            migrationBuilder.DropIndex(
                name: "IX_Flames_BirthID",
                table: "Flames");

            migrationBuilder.DropIndex(
                name: "IX_Breeding_ChildID",
                table: "Breeding");

            migrationBuilder.DropIndex(
                name: "IX_Breeding_FirstParentID",
                table: "Breeding");

            migrationBuilder.DropIndex(
                name: "IX_Breeding_FlameID",
                table: "Breeding");

            migrationBuilder.DropIndex(
                name: "IX_Breeding_SecondParentID",
                table: "Breeding");

            migrationBuilder.DropColumn(
                name: "FirstParentID",
                table: "Breeding");

            migrationBuilder.DropColumn(
                name: "FlameID",
                table: "Breeding");

            migrationBuilder.DropColumn(
                name: "SecondParentID",
                table: "Breeding");

            migrationBuilder.CreateTable(
                name: "BreedingFlame",
                columns: table => new
                {
                    BreedingsID = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentsID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreedingFlame", x => new { x.BreedingsID, x.ParentsID });
                    table.ForeignKey(
                        name: "FK_BreedingFlame_Breeding_BreedingsID",
                        column: x => x.BreedingsID,
                        principalTable: "Breeding",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BreedingFlame_Flames_ParentsID",
                        column: x => x.ParentsID,
                        principalTable: "Flames",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Flames_BirthID",
                table: "Flames",
                column: "BirthID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BreedingFlame_ParentsID",
                table: "BreedingFlame",
                column: "ParentsID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BreedingFlame");

            migrationBuilder.DropIndex(
                name: "IX_Flames_BirthID",
                table: "Flames");

            migrationBuilder.AddColumn<int>(
                name: "FirstParentID",
                table: "Breeding",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FlameID",
                table: "Breeding",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SecondParentID",
                table: "Breeding",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

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
                name: "FK_Breeding_Flames_ChildID",
                table: "Breeding",
                column: "ChildID",
                principalTable: "Flames",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Breeding_Flames_FirstParentID",
                table: "Breeding",
                column: "FirstParentID",
                principalTable: "Flames",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Breeding_Flames_FlameID",
                table: "Breeding",
                column: "FlameID",
                principalTable: "Flames",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Breeding_Flames_SecondParentID",
                table: "Breeding",
                column: "SecondParentID",
                principalTable: "Flames",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
