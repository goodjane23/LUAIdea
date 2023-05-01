using Lua_IDEA.Data;
using Lua_IDEA.Data.Entities;
using Lua_IDEA.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lua_IDEA.Services
{
    class FileService
    {
        
        public static async Task<IEnumerable<string>> GetFavoriteMacros()
        {
            await using var appDbContext = new AppDbContext();

            var favoriteMacros = await appDbContext.FavoriteFiles
                .Select(x => x.Path)
                .ToListAsync();
           
            return favoriteMacros;
        }

        public static async Task<IEnumerable<string>> AddToFavorite(string path)
        {
            await using var appDbContext = new AppDbContext();
            FavoriteFile favoriteFile = new FavoriteFile();
            favoriteFile.Path = path;
            await appDbContext.FavoriteFiles.AddAsync(favoriteFile);
            await appDbContext.SaveChangesAsync();
            return await GetFavoriteMacros();
        }

        public static async Task<IEnumerable<string>> RemoveFromFavorite(string path)
        {
            await using var appDbContext = new AppDbContext();
            var favoriteMacros = appDbContext.FavoriteFiles.FirstOrDefault(x => x.Path == path);
            if (favoriteMacros == null) return null;
            
            appDbContext.FavoriteFiles.Remove(favoriteMacros);

            await appDbContext.SaveChangesAsync();

            return await GetFavoriteMacros();
        }
    }
}
