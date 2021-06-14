using Microsoft.EntityFrameworkCore.Migrations;

namespace FlameReactor.DB.Migrations
{
    public partial class Adjustments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BreedingFlame_Breeding_BreedingsID",
                table: "BreedingFlame");

            migrationBuilder.DropForeignKey(
                name: "FK_Flames_Breeding_BirthID",
                table: "Flames");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Vote",
                table: "Vote");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InteractionEvent",
                table: "InteractionEvent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Breeding",
                table: "Breeding");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AccessEvent",
                table: "AccessEvent");

            migrationBuilder.RenameTable(
                name: "Vote",
                newName: "Votes");

            migrationBuilder.RenameTable(
                name: "InteractionEvent",
                newName: "InteractionEvents");

            migrationBuilder.RenameTable(
                name: "Breeding",
                newName: "Breedings");

            migrationBuilder.RenameTable(
                name: "AccessEvent",
                newName: "AccessEvents");

            migrationBuilder.AddColumn<int>(
                name: "Adjustment",
                table: "Votes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Votes",
                table: "Votes",
                columns: new[] { "IPAddress", "FlameId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_InteractionEvents",
                table: "InteractionEvents",
                columns: new[] { "Timestamp", "IPAddress" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Breedings",
                table: "Breedings",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccessEvents",
                table: "AccessEvents",
                columns: new[] { "Timestamp", "IPAddress" });

            migrationBuilder.AddForeignKey(
                name: "FK_BreedingFlame_Breedings_BreedingsID",
                table: "BreedingFlame",
                column: "BreedingsID",
                principalTable: "Breedings",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Flames_Breedings_BirthID",
                table: "Flames",
                column: "BirthID",
                principalTable: "Breedings",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BreedingFlame_Breedings_BreedingsID",
                table: "BreedingFlame");

            migrationBuilder.DropForeignKey(
                name: "FK_Flames_Breedings_BirthID",
                table: "Flames");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Votes",
                table: "Votes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InteractionEvents",
                table: "InteractionEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Breedings",
                table: "Breedings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AccessEvents",
                table: "AccessEvents");

            migrationBuilder.DropColumn(
                name: "Adjustment",
                table: "Votes");

            migrationBuilder.RenameTable(
                name: "Votes",
                newName: "Vote");

            migrationBuilder.RenameTable(
                name: "InteractionEvents",
                newName: "InteractionEvent");

            migrationBuilder.RenameTable(
                name: "Breedings",
                newName: "Breeding");

            migrationBuilder.RenameTable(
                name: "AccessEvents",
                newName: "AccessEvent");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vote",
                table: "Vote",
                columns: new[] { "IPAddress", "FlameId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_InteractionEvent",
                table: "InteractionEvent",
                columns: new[] { "Timestamp", "IPAddress" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Breeding",
                table: "Breeding",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccessEvent",
                table: "AccessEvent",
                columns: new[] { "Timestamp", "IPAddress" });

            migrationBuilder.AddForeignKey(
                name: "FK_BreedingFlame_Breeding_BreedingsID",
                table: "BreedingFlame",
                column: "BreedingsID",
                principalTable: "Breeding",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Flames_Breeding_BirthID",
                table: "Flames",
                column: "BirthID",
                principalTable: "Breeding",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
