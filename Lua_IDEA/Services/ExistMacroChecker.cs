using Lua_IDEA.Models;
using Microsoft.UI.Xaml.Controls;
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
    string error;
    public List<LuaFile> ExistMacros { get; set; } = new();
    public List<LuaFile> ExistBackground { get; } = new();

    public ExistMacroChecker()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var pm = $@"{appDataPath}\Purelogic\Pumotix\Server\Macros\";
        CreateDirectory(pm);
    }

    private async Task CreateDirectory(string pm)
    {
        try
        {
            var rootDirectoryInfo = new DirectoryInfo(pm);
            ExistMacros = await GetFiles(rootDirectoryInfo);
            var node = await CreateDirectoryNode(rootDirectoryInfo);

        }
        catch (Exception)
        {

        }
    }

    private async Task<List<LuaFile>> GetFiles(DirectoryInfo path)
    {
        List<LuaFile> files = new List<LuaFile>();

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

            files.Add(luaFile);
        }
        return files;
    }
   
    private async Task<List<DirectoryInfo>> CreateDirectoryNode(DirectoryInfo directoryInfo)
    {
        List<DirectoryInfo> folders = new List<DirectoryInfo>();
        var dInfo = directoryInfo.GetDirectories();

        return dInfo.ToList();

    }
}
