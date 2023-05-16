using Lua_IDEA.Data;
using Lua_IDEA.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lua_IDEA.Services;

public class FilesServise
{
    private readonly IDbContextFactory<AppDbContext> contextFactory;

    public FilesServise(IDbContextFactory<AppDbContext> contextFactory)
    {
        this.contextFactory = contextFactory;
    }

    public async Task<IEnumerable<string>> GetFavoriteMacros()
    {
        await using var appDbContext = await contextFactory.CreateDbContextAsync();

        var favoriteMacros = await appDbContext.SavedPathFiles
            .AsNoTracking()
            .Where(x => x.IsFavorite)
            .Select(x => x.Path)
            .ToListAsync();
       
        return favoriteMacros.TakeLast(30);
    }

    public async Task<IEnumerable<string>> GetRecentMacros()
    {
        await using var appDbContext = await contextFactory.CreateDbContextAsync();

        var recentMacros = await appDbContext.SavedPathFiles
            .AsNoTracking()
            .Where(x => x.IsRecent)
            .Select(x => x.Path)
            .ToListAsync();

        return recentMacros.TakeLast(10);
    }

    public async Task AddToFavorite(string path)
    {
        await using var appDbContext = await contextFactory.CreateDbContextAsync();

        var favoriteFile = new SavedFile
        {            
            Path = path,
            IsFavorite = true,
            IsRecent = false
        };

        await appDbContext.SavedPathFiles.AddAsync(favoriteFile);
        await appDbContext.SaveChangesAsync();
    }
    public async Task AddToRecent(string path)
    {
        await using var appDbContext = await contextFactory.CreateDbContextAsync();

        var recentFile = new SavedFile
        {
            Path = path,
            IsFavorite = false,
            IsRecent = true,
        };

        await appDbContext.SavedPathFiles.AddAsync(recentFile);
        await appDbContext.SaveChangesAsync();
    }

    public async Task RemoveFromFavorite(string path)
    {
        await using var appDbContext = await contextFactory.CreateDbContextAsync();

        var favoriteMacros = appDbContext.SavedPathFiles
            .FirstOrDefault(x => x.Path == path);

        if (favoriteMacros is null)
            return;
        
        appDbContext.SavedPathFiles.Remove(favoriteMacros);
        await appDbContext.SaveChangesAsync();
    }
}
