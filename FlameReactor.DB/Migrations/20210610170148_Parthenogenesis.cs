using Microsoft.EntityFrameworkCore.Migrations;

namespace FlameReactor.DB.Migrations
{
    public partial class Parthenogenesis : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flames_Breedings_BirthID",
                table: "Flames");

            migrationBuilder.AlterColumn<int>(
                name: "BirthID",
                table: "Flames",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Flames_Breedings_BirthID",
                table: "Flames",
                column: "BirthID",
                principalTable: "Breedings",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flames_Breedings_BirthID",
                table: "Flames");

            migrationBuilder.AlterColumn<int>(
                name: "BirthID",
                table: "Flames",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Flames_Breedings_BirthID",
                table: "Flames",
                column: "BirthID",
                principalTable: "Breedings",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
