using Lua_IDEA.Data;
using Lua_IDEA.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lua_IDEA.Services;

public class FilesOnDBService
{
    private readonly IDbContextFactory<AppDbContext> contextFactory;

    public FilesOnDBService(IDbContextFactory<AppDbContext> contextFactory)
    {
        this.contextFactory = contextFactory;
    }

    public async Task<IEnumerable<string>> GetFavoriteMacros()
    {
        await using var appDbContext = await contextFactory.CreateDbContextAsync();

        var favoriteMacros = await appDbContext.FavoriteFiles            
            .Where(x => x.IsFavorite == true)
            .Select(x => x.Path)
            .ToListAsync();
       
        return favoriteMacros;
    }

    public async Task<IEnumerable<string>> GetRecentMacros()
    {
        await using var appDbContext = await contextFactory.CreateDbContextAsync();

        var recentMacros = await appDbContext.FavoriteFiles
            .Where(x => x.IsFavorite == false)
            .Select(x => x.Path)
            .ToListAsync();

        recentMacros = recentMacros.GetRange(recentMacros.Count-10, recentMacros.Count);
        return recentMacros;
    }

    public async Task AddToFavorite(string path)
    {
        await using var appDbContext = await contextFactory.CreateDbContextAsync();

        var favoriteFile = new SavedFile
        {
            Path = path
        };

        await appDbContext.FavoriteFiles.AddAsync(favoriteFile);
        await appDbContext.SaveChangesAsync();
    }

    public async Task AddToRecent(string path)
    {
        await using var appDbContext = await contextFactory.CreateDbContextAsync();

        var recentFile = new SavedFile
        {
            Path = path,
            IsFavorite = false
        };

        await appDbContext.FavoriteFiles.AddAsync(recentFile);
        await appDbContext.SaveChangesAsync();
    }

    public async Task RemoveFromFavorite(string path)
    {
        await using var appDbContext = await contextFactory.CreateDbContextAsync();

        var favoriteMacros = appDbContext.FavoriteFiles
            .FirstOrDefault(x => x.Path == path);

        if (favoriteMacros is null)
            return;
        
        appDbContext.FavoriteFiles.Remove(favoriteMacros);
        await appDbContext.SaveChangesAsync();
    }
}
