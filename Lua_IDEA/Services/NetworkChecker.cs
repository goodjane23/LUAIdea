using System.Net.Http;

namespace Lua_IDEA.Services;

public class NetworkChecker
{
    public bool IsInternetAvailable(HttpClient httpClient)
    {
        return true;
    }
}
