using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameServer.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO public.""User"" (""Id"", ""Name"", ""Email"", ""Password"")
VALUES (1, 'Arseniy', 'admin', 'admin');
INSERT INTO public.""User"" (""Id"", ""Name"", ""Email"", ""Password"")
VALUES (2, 'Client', 'client', 'admin');
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM public.\"User\";");
        }
    }
}
