using MoonSharp.Interpreter;
using Lua_IDEA.Contracts.Services;

namespace Lua_IDEA.Services;

public class SyntaxChecker
{
	private const string SettingsKey = "AppSyntaxCheckEnabled";
	
	public bool IsSyntaxCheckEnabled { get; set; }

	private readonly ILocalSettingsService localSettingsService;
	
	public SyntaxChecker(ILocalSettingsService localSettingsService)
	{
		this.localSettingsService = localSettingsService;
	}

	public async Task SetSyntaxChecking(bool param)
	{
		IsSyntaxCheckEnabled = param;
		await localSettingsService.SaveSettingAsync(SettingsKey, param);
	}
	
	public async Task InitializeAsync()
	{
		IsSyntaxCheckEnabled = await LoadCheckingFromSettingsAsync();
	}
	
    public string CheckSyntax(string text)
    {
        var script = new Script();
        var result = "";

		try
		{
			var res = Script.RunString(text);
        }
		catch (Exception ex)
		{
            return ex.Message;
		}

        return result;
    }
    
    private async Task<bool> LoadCheckingFromSettingsAsync()
    {
	    return await localSettingsService.ReadSettingAsync<bool>(SettingsKey);
    }
}
