using Lua_IDEA.Contracts.Services.Models;
using Lua_IDEA.Models;

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

    public async Task<IEnumerable<PumotixFile>> GetInnerMacros()
    {
        var files = Directory.GetFiles(path);
        var result = new List<PumotixFile>(files.Length);
        
        foreach (var file in files.Where(x => x.EndsWith(".pm")))
        {
            var fileInfo = new FileInfo(file);
            using var fileStream = fileInfo.OpenText();
            
            result.Add(new PumotixFile()
            {
                Name = fileInfo.Name,
                Path = fileInfo.FullName,
                Content = await fileStream.ReadToEndAsync()
            });
        }

        return result;
    }

    public async Task<IEnumerable<PumotixFile>> GetInnerBackgroundOperations()
    {
        var files = Directory.GetFiles(path+"Background");
        var result = new List<PumotixFile>(files.Length);

        foreach (var file in files.Where(x => x.EndsWith(".bm")))
        {
            var fileInfo = new FileInfo(file);
            using var fileStream = fileInfo.OpenText();
            
            result.Add(new PumotixFile()
            {
                Name = fileInfo.Name,
                Path = fileInfo.FullName,
                Content = await fileStream.ReadToEndAsync()
            });
        }

        return result;
    }
}
