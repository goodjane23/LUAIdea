using Lua_IDEA.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lua_IDEA.Services;

public class ExistMacroChecker
{
    private List<LuaFile> totalFiles = new();

    string error;
    public List<LuaFile> ExistMacros { get; set; } = new();
    public List<LuaFile> ExistBackground { get; set; } = new();

    public ExistMacroChecker()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var pm = $@"{appDataPath}\Purelogic\Pumotix\Server\Macros\";
        FindDirectory(pm);
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
            await SortingMacro();
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

            totalFiles.Add(luaFile);
        }
    }

    private async Task SortingMacro()
    {
        ExistMacros = totalFiles.FindAll(x => x.Name.Contains(".pm"));
        ExistBackground = totalFiles.FindAll(x => x.Name.Contains(".bm"));
    }
}