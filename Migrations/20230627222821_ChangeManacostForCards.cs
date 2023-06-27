using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameServer.Migrations
{
    /// <inheritdoc />
    public partial class ChangeManacostForCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE ""Card"" SET ""Manacost"" = 3 WHERE ""Id"" IN (3, 27);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE ""Card"" SET ""Manacost"" = 2 WHERE ""Id"" IN (3, 27);");
        }
    }
}
