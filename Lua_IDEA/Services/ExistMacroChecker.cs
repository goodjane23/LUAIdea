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
    public ObservableCollection<LuaFile> ExistMacros { get; } = new();

    public ExistMacroChecker()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var pm = $@"{appDataPath}\Purelogic\Pumotix\Server\Macros\";
        Smth(pm);
    }

    private async Task Smth(string pm)
    {
        try
        {
            var rootDirectoryInfo = new DirectoryInfo(pm);
            var node = await CreateDirectoryNode(rootDirectoryInfo);
            
           
        }
        catch (Exception)
        {

        }
    }

    private async Task<List<DirectoryInfo>> CreateDirectoryNode(DirectoryInfo directoryInfo)
    {
        List<DirectoryInfo> files = new List<DirectoryInfo>();

        //Получаем папки которые лежат внутри папки макросов
        foreach (var directory in directoryInfo.GetDirectories())
        {
            files.Add(directory);
        }

        foreach (var file in directoryInfo.GetFiles())
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

            ExistMacros.Add(luaFile);
        }
        return files;
    }
}
