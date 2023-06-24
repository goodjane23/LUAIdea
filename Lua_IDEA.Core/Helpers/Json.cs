using System.Text.Json;

namespace Lua_IDEA.Core.Helpers;

public static class Json
{
    public static async Task<T> ToObjectAsync<T>(string value)
    {
        return await Task.Run<T>(() =>
        {
            try
            {
                return JsonSerializer.Deserialize<T>(value);
            }
            catch (Exception)
            {
                return default(T);
            }
        });
    }

    public static async Task<string> StringifyAsync(object value)
    {
        return await Task.Run<string>(() =>
        {
            return JsonSerializer.Serialize(value);
        });
    }
}
