using System.Reflection;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lua_IDEA.Contracts.Services;
using Lua_IDEA.Helpers;
using Lua_IDEA.Helpers.Extensions;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;
using Lua_IDEA.Services;

namespace Lua_IDEA.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    [ObservableProperty]
    private ElementTheme elementTheme;

    [ObservableProperty]
    private bool isSyntaxCheckEnabled;

    [ObservableProperty]
    private string versionDescription;

    [ObservableProperty]
    private bool saveMacroInPmtx;

    [ObservableProperty]
    private bool saveBackInPmtx;

    private readonly IThemeSelectorService themeSelectorService;
    private readonly INavigationService navigationService;
    private readonly SyntaxChecker syntaxChecker;

    public SettingsViewModel(
        IThemeSelectorService themeSelectorService,
        INavigationService navigationService,
        SyntaxChecker syntaxChecker)
    {
        this.themeSelectorService = themeSelectorService;
        this.navigationService = navigationService;
        this.syntaxChecker = syntaxChecker;

        ElementTheme = themeSelectorService.Theme;
        IsSyntaxCheckEnabled = syntaxChecker.IsSyntaxCheckEnabled;
        VersionDescription = GetVersionDescription();
    }

    [RelayCommand]
    private void GoBack()
    {
        navigationService.GoBack();
    }

    [RelayCommand]
    private async Task SwitchTheme(ElementTheme theme)
    {
        if (ElementTheme != theme)
        {
            ElementTheme = theme;
            await themeSelectorService.SetThemeAsync(theme);
        }
    }

    [RelayCommand]
    private async Task SwitchSyntaxCheck()
    {
        await syntaxChecker.SetSyntaxChecking(IsSyntaxCheckEnabled);
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}
