using Microsoft.EntityFrameworkCore.Migrations;

namespace FlameReactor.DB.Migrations
{
    public partial class TweetRecordFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TweetRecord_Flames_FlameID",
                table: "TweetRecord");

            migrationBuilder.RenameColumn(
                name: "FlameID",
                table: "TweetRecord",
                newName: "OwnerID");

            migrationBuilder.RenameIndex(
                name: "IX_TweetRecord_FlameID",
                table: "TweetRecord",
                newName: "IX_TweetRecord_OwnerID");

            migrationBuilder.AddForeignKey(
                name: "FK_TweetRecord_Flames_OwnerID",
                table: "TweetRecord",
                column: "OwnerID",
                principalTable: "Flames",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TweetRecord_Flames_OwnerID",
                table: "TweetRecord");

            migrationBuilder.RenameColumn(
                name: "OwnerID",
                table: "TweetRecord",
                newName: "FlameID");

            migrationBuilder.RenameIndex(
                name: "IX_TweetRecord_OwnerID",
                table: "TweetRecord",
                newName: "IX_TweetRecord_FlameID");

            migrationBuilder.AddForeignKey(
                name: "FK_TweetRecord_Flames_FlameID",
                table: "TweetRecord",
                column: "FlameID",
                principalTable: "Flames",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
