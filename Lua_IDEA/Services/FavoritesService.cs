using Lua_IDEA.Data;
using Lua_IDEA.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lua_IDEA.Services;

public class FavoritesService
{
    private readonly IDbContextFactory<AppDbContext> contextFactory;

    public FavoritesService(IDbContextFactory<AppDbContext> contextFactory)
    {
        this.contextFactory = contextFactory;
    }

    public  async Task<IEnumerable<string>> GetFavoriteMacros()
    {
        await using var appDbContext = await contextFactory.CreateDbContextAsync();

        var favoriteMacros = await appDbContext.FavoriteFiles
            .Select(x => x.Path)
            .ToListAsync();
       
        return favoriteMacros;
    }

    public async Task AddToFavorite(string path)
    {
        await using var appDbContext = await contextFactory.CreateDbContextAsync();

        var favoriteFile = new FavoriteFile
        {
            Path = path
        };

        await appDbContext.FavoriteFiles.AddAsync(favoriteFile);
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
