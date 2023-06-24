using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameServer.Migrations
{
    /// <inheritdoc />
    public partial class ManaUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Mana",
                table: "Player",
                newName: "ManaCurrent");

            migrationBuilder.AddColumn<int>(
                name: "ManaCommon",
                table: "Player",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ManaCommon",
                table: "Player");

            migrationBuilder.RenameColumn(
                name: "ManaCurrent",
                table: "Player",
                newName: "Mana");
        }
    }
}
