using Lua_IDEA.Data;
using Lua_IDEA.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace Lua_IDEA.Services;

public class FilesService
{
    private readonly IDbContextFactory<AppDbContext> contextFactory;

    public FilesService(IDbContextFactory<AppDbContext> contextFactory)
    {
        this.contextFactory = contextFactory;
    }

    public async Task<IEnumerable<FavoriteFile>> GetFavoriteMacros()
    {
        await using var appDbContext = await contextFactory.CreateDbContextAsync();

        var favoriteMacros = await appDbContext.FavoriteFiles
            .AsNoTracking()
            .ToListAsync();
       
        return favoriteMacros.TakeLast(30);
    }

    public async Task<IEnumerable<RecentFile>> GetRecentMacros()
    {
        await using var appDbContext = await contextFactory.CreateDbContextAsync();

        var recentMacros = await appDbContext.RecentFiles
            .AsNoTracking()
            .ToListAsync();

        return recentMacros.TakeLast(10);
    }

    public async Task AddToFavorite(string path)
    {
        await using var appDbContext = await contextFactory.CreateDbContextAsync();

        var file = new FavoriteFile{ Path = path };

        await appDbContext.FavoriteFiles.AddAsync(file);
        await appDbContext.SaveChangesAsync();
    }

    public async Task AddToRecent(string path)
    {
        RecentFile file = new RecentFile { Path = path };
        await using var appDbContext = await contextFactory.CreateDbContextAsync();

        var recentMacros = await appDbContext.RecentFiles
           .AsNoTracking()
           .ToListAsync();

        var isIn = recentMacros.FirstOrDefault(x=> x.Path.Equals(path,StringComparison.OrdinalIgnoreCase));

        if (isIn != null) await RemoveFromRecent(path);

        await appDbContext.RecentFiles.AddAsync(file);
        await appDbContext.SaveChangesAsync();
    }

    public async Task RemoveFromFavorite(string path)
    {
        await using var appDbContext = await contextFactory.CreateDbContextAsync();

        var favoriteFile = await appDbContext.FavoriteFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Path == path);

        if (favoriteFile is null)
            return;
        
        appDbContext.FavoriteFiles.Remove(favoriteFile);
        await appDbContext.SaveChangesAsync();
    }

    public async Task RemoveFromRecent(string path)
    {
        await using var appDbContext = await contextFactory.CreateDbContextAsync();

        var recentFile = await appDbContext.RecentFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Path == path);

        if (recentFile is null)
            return;

        appDbContext.RecentFiles.Remove(recentFile);
        await appDbContext.SaveChangesAsync();
    }
}
