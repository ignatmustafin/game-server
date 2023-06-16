using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GameServer.Migrations
{
    /// <inheritdoc />
    public partial class unionPlayerCardAndPlayerUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Field1CardId",
                table: "Player",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Field2CardId",
                table: "Player",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Field3CardId",
                table: "Player",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Field4CardId",
                table: "Player",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Card",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Manacost = table.Column<int>(type: "integer", nullable: false),
                    Hp = table.Column<int>(type: "integer", nullable: false),
                    Damage = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Card", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerCard",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    CardId = table.Column<int>(type: "integer", nullable: false),
                    CardIn = table.Column<int>(type: "integer", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerCard", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerCard_Card_CardId",
                        column: x => x.CardId,
                        principalTable: "Card",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerCard_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerCard_CardId",
                table: "PlayerCard",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerCard_PlayerId",
                table: "PlayerCard",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerCard");

            migrationBuilder.DropTable(
                name: "Card");

            migrationBuilder.DropColumn(
                name: "Field1CardId",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Field2CardId",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Field3CardId",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Field4CardId",
                table: "Player");
        }
    }
}
