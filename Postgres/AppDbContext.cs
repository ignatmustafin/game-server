using GameServer.Models;
using Microsoft.EntityFrameworkCore;

namespace GameServer.Postgres;

public class AppDbContext : DbContext
{
    public DbSet<User> User { get; set; }
    public DbSet<Game> Game { get; set; }
    public DbSet<Player> Player { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=postgres;Port=5432;Database=game;Username=macbookair;Password=admin");
    }
}