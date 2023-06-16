using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameServer.Migrations
{
    /// <inheritdoc />
    public partial class TurnEndedAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TurnEnded",
                table: "Player",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Card",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TurnEnded",
                table: "Player");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Card",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
