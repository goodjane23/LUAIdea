using Microsoft.UI.Xaml;

namespace Lua_IDEA.Contracts.Services;

public interface IThemeSelectorService
{
    ElementTheme Theme { get; }

    Task InitializeAsync();
    Task SetThemeAsync(ElementTheme theme);
    Task SetRequestedThemeAsync();
}
