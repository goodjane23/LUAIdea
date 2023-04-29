using Lua_IDEA.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace Lua_IDEA.Data;

public class AppDbContext : DbContext
{
    public DbSet<CommandCategory> CommandCategory { get; set; }
    public DbSet<Command> Commands { get; set; }
    public DbSet<FavoriteFile> FavoriteFiles { get; set; }

    public AppDbContext() => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=helloapp.db");
    }
}
