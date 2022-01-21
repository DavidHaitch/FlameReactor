using Microsoft.EntityFrameworkCore.Migrations;

namespace FlameReactor.DB.Migrations
{
    public partial class Referrer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Referrer",
                table: "AccessEvents",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Referrer",
                table: "AccessEvents");
        }
    }
}
