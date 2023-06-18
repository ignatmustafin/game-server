using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameServer.Migrations
{
    /// <inheritdoc />
    public partial class PlayerCardUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PlayerCard");

            migrationBuilder.AddColumn<int>(
                name: "Damage",
                table: "PlayerCard",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Hp",
                table: "PlayerCard",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDead",
                table: "PlayerCard",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Manacost",
                table: "PlayerCard",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "PlayerCard",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "PlayerCard",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Damage",
                table: "PlayerCard");

            migrationBuilder.DropColumn(
                name: "Hp",
                table: "PlayerCard");

            migrationBuilder.DropColumn(
                name: "IsDead",
                table: "PlayerCard");

            migrationBuilder.DropColumn(
                name: "Manacost",
                table: "PlayerCard");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "PlayerCard");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "PlayerCard");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PlayerCard",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
