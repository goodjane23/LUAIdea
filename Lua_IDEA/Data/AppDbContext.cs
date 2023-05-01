using Lua_IDEA.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lua_IDEA.Data;

public sealed class AppDbContext : DbContext
{
    public DbSet<CommandCategory> CommandCategory { get; set; }
    public DbSet<Command> Commands { get; set; }
    public DbSet<FavoriteFile> FavoriteFiles { get; set; }

    public AppDbContext()
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=database.db");
    }
}
