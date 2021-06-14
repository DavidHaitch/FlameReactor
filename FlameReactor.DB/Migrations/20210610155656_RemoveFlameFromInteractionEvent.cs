using Microsoft.EntityFrameworkCore.Migrations;

namespace FlameReactor.DB.Migrations
{
    public partial class RemoveFlameFromInteractionEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InteractionEvent_Flames_FlameId",
                table: "InteractionEvent");

            migrationBuilder.DropIndex(
                name: "IX_InteractionEvent_FlameId",
                table: "InteractionEvent");

            migrationBuilder.DropColumn(
                name: "FlameId",
                table: "InteractionEvent");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FlameId",
                table: "InteractionEvent",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InteractionEvent_FlameId",
                table: "InteractionEvent",
                column: "FlameId");

            migrationBuilder.AddForeignKey(
                name: "FK_InteractionEvent_Flames_FlameId",
                table: "InteractionEvent",
                column: "FlameId",
                principalTable: "Flames",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
