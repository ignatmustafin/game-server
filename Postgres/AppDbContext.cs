using GameServer.Models;
using Microsoft.EntityFrameworkCore;

namespace GameServer.Postgres;

public class AppDbContext : DbContext
{
    public DbSet<User> User { get; set; }
    public DbSet<Game> Game { get; set; }
    public DbSet<Player> Player { get; set; }
    public DbSet<Card> Card { get; set; }
    public DbSet<PlayerCard> PlayerCard { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=postgres;Port=5432;Database=game;Username=macbookair;Password=admin");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }

    public void InitializeDatabase()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql("Host=postgres;Port=5432;Database=game;Username=macbookair;Password=admin");

        using (var dbContext = new AppDbContext(optionsBuilder.Options))
        {
            if (dbContext.Card.Count() == 0)
            {
                string sql = @"
    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (1, 'card1', 0, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (5, 'card5', 1, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (3, 'card3', 2, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (6, 'card6', 2, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (2, 'card2', 1, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (4, 'card4', 0, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (7, 'card7', 3, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (8, 'card8', 0, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (9, 'card9', 1, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (10, 'card10', 2, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (11, 'card11', 3, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (12, 'card12', 0, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (13, 'card13', 1, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (14, 'card14', 2, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (15, 'card15', 3, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (16, 'card16', 0, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (17, 'card17', 1, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (18, 'card18', 2, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (19, 'card19', 3, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (20, 'card20', 0, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (21, 'card21', 1, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (22, 'card22', 2, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (23, 'card23', 3, 1, 1, 1);

    INSERT INTO public.""Card"" (""Id"", ""Name"", ""Type"", ""Manacost"", ""Hp"", ""Damage"")
    VALUES (24, 'card24', 0, 1, 1, 1);
";


                dbContext.Database.ExecuteSqlRaw(sql);
            }
        }
    }
}