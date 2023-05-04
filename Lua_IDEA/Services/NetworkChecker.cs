using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lua_IDEA.Services;

public class NetworkChecker
{
    public async Task<bool> IsNecessaryResourceAvailable(HttpClient httpClient, string? address = null)
    {
        address ??= "http://doc.pumotix.ru/";

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
