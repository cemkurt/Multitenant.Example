using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Multitenant.Example.Persistence.Migrations
{
    public partial class sel1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Sed",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sed",
                table: "Products");
        }
    }
}
