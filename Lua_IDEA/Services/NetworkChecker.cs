using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lua_IDEA.Services;

public class NetworkChecker
{
    public async Task<bool> IsInternetAvailable(HttpClient httpClient, string? address = null)
    {
        address ??= "https://www.google.com/";

		try
		{
            var response = await httpClient.GetAsync(address);
            return response.IsSuccessStatusCode;
        }
		catch (Exception)
		{
            return false;
		}
    }
}
