using Lua_IDEA.Models;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Lua_IDEA.Services;

public class ExistMacroService
{
    private readonly string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly string path;

    string error;

    public ExistMacroService()
    {
        path = $@"{appDataPath}\Purelogic\Pumotix\Server\Macros\";
    }

    private async Task FindDirectory(string directory)
    {        
        try
        {
            //mainfolder - макросы основные лежат тут и пользовательские то же тут.
            //Внутри должна быть папка с ФО
            var rootDirectoryInfo = new DirectoryInfo(directory);

            // список для внутренних папок
            List<DirectoryInfo> folders = new List<DirectoryInfo>();

            folders.AddRange(rootDirectoryInfo.GetDirectories().ToList());
            folders.Add(rootDirectoryInfo);

            foreach (var folder in folders)
            {
                await GetFiles(folder);
            }
           
        }
        catch (Exception)
        {

        }
    }

    private async Task GetFiles(DirectoryInfo path)
    {
        foreach (var file in path.GetFiles())
        {
            using var sr = new StreamReader(file.FullName);

            var script = new Script();
            var content = await sr.ReadToEndAsync();

            var code = content.Replace("require(", "---require(");

            try
            {
                script.DoString(code);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            LuaFile luaFile = new LuaFile
            {
                Path = file.FullName,
                IsSaved = true,
                IsFavorite = true,
                Name = file.Name,
                Content = content,
                Errors = error,
            };


        }
    }

    public async Task<IEnumerable<LuaFile>> GetInnerMacros()
    {
        return null;
    }

    public async Task<IEnumerable<LuaFile>> GetInnerBackgroudOperations()
    {
        return null;
    }
}
