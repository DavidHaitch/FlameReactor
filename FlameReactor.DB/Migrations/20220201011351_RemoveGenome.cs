using Microsoft.EntityFrameworkCore.Migrations;

namespace FlameReactor.DB.Migrations
{
    public partial class RemoveGenome : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Genome",
                table: "Flames");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Genome",
                table: "Flames",
                type: "TEXT",
                nullable: true);
        }
    }
}
