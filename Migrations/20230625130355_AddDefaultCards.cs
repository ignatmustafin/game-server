using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameServer.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (1, 'Munchkin warrior', 0, 1, 4, 1, '/Assets/Images/munchkin-warrior.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (2, 'Maine Coon warrior', 0, 2, 8, 2, '/Assets/Images/maine-coon-warrior.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (3, 'Sphinx warrior', 0, 2, 12, 3, '/Assets/Images/sphinx-warrior.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (4, 'Munchkin archer', 1, 2, 3, 2, '/Assets/Images/munchkin-archer.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (5, 'Maine Coon archer', 1, 3, 5, 3, '/Assets/Images/maine-coon-archer.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (6, 'Sphinx archer', 1, 4, 7, 4, '/Assets/Images/sphinx-archer.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (7, 'Munchkin shooter', 2, 2, 3, 2, '/Assets/Images/munchkin-shooter.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (8, 'Maine Coon shooter', 2, 3, 5, 3, '/Assets/Images/maine-coon-shooter.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (9, 'Sphinx shooter', 2, 4, 7, 4, '/Assets/Images/sphinx-shooter.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (10, 'Munchkin energy', 3, 3, 3, 2, '/Assets/Images/munchkin-energy.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (11, 'Maine Coon energy', 3, 4, 4, 3, '/Assets/Images/maine-coon-energy.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (12, 'Sphinx energy', 3, 5, 6, 4, '/Assets/Images/sphinx-energy.png');


INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (13, 'Munchkin warrior', 0, 1, 4, 1, '/Assets/Images/munchkin-warrior.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (14, 'Maine Coon warrior', 0, 2, 8, 2, '/Assets/Images/maine-coon-warrior.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (15, 'Sphinx warrior', 0, 2, 12, 3, '/Assets/Images/sphinx-warrior.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (16, 'Munchkin archer', 1, 2, 3, 2, '/Assets/Images/munchkin-archer.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (17, 'Maine Coon archer', 1, 3, 5, 3, '/Assets/Images/maine-coon-archer.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (18, 'Sphinx archer', 1, 4, 7, 4, '/Assets/Images/sphinx-archer.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (19, 'Munchkin shooter', 2, 2, 3, 2, '/Assets/Images/munchkin-shooter.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (20, 'Maine Coon shooter', 2, 3, 5, 3, '/Assets/Images/maine-coon-shooter.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (21, 'Sphinx shooter', 2, 4, 7, 4, '/Assets/Images/sphinx-shooter.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (22, 'Munchkin energy', 3, 3, 3, 2, '/Assets/Images/munchkin-energy.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (23, 'Maine Coon energy', 3, 4, 4, 3, '/Assets/Images/maine-coon-energy.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (24, 'Sphinx energy', 3, 5, 6, 4, '/Assets/Images/sphinx-energy.png');


INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (25, 'Corgi warrior', 0, 1, 4, 1, '/Assets/Images/corgi-warrior.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (26, 'Terrier warrior', 0, 2, 8, 2, '/Assets/Images/terrier-warrior.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (27, 'Husky warrior', 0, 2, 12, 3, '/Assets/Images/husky-warrior.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (28, 'Corgi archer', 1, 2, 3, 2, '/Assets/Images/corgi-archer.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (29, 'terrier archer', 1, 3, 5, 3, '/Assets/Images/terrier-archer.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (30, 'Husky archer', 1, 4, 7, 4, '/Assets/Images/husky-archer.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (31, 'Corgi shooter', 2, 2, 3, 2, '/Assets/Images/corgi-shooter.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (32, 'Terrier shooter', 2, 3, 5, 3, '/Assets/Images/terrier-shooter.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (33, 'Husky shooter', 2, 4, 7, 4, '/Assets/Images/husky-shooter.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (34, 'Corgi energy', 3, 3, 3, 2, '/Assets/Images/corgi-energy.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (35, 'terrier energy', 3, 4, 4, 3, '/Assets/Images/terrier-energy.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (36, 'Husky energy', 3, 5, 6, 4, '/Assets/Images/husky-energy.png');


INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (37, 'Corgi warrior', 0, 1, 4, 1, '/Assets/Images/corgi-warrior.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (38, 'Terrier warrior', 0, 2, 8, 2, '/Assets/Images/terrier-warrior.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (39, 'Husky warrior', 0, 2, 12, 3, '/Assets/Images/husky-warrior.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (40, 'Corgi archer', 1, 2, 3, 2, '/Assets/Images/corgi-archer.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (41, 'terrier archer', 1, 3, 5, 3, '/Assets/Images/terrier-archer.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (42, 'Husky archer', 1, 4, 7, 4, '/Assets/Images/husky-archer.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (43, 'Corgi shooter', 2, 2, 3, 2, '/Assets/Images/corgi-shooter.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (44, 'Terrier shooter', 2, 3, 5, 3, '/Assets/Images/terrier-shooter.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (45, 'Husky shooter', 2, 4, 7, 4, '/Assets/Images/husky-shooter.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (46, 'Corgi energy', 3, 3, 3, 2, '/Assets/Images/corgi-energy.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (47, 'terrier energy', 3, 4, 4, 3, '/Assets/Images/terrier-energy.png');
INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"", ""ImageUrl"")
VALUES (48, 'Husky energy', 3, 5, 6, 4, '/Assets/Images/husky-energy.png');
");
            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM public.\"Card\";");
        }
    }
}
