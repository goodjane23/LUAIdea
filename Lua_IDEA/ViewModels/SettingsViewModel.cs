using System.Reflection;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lua_IDEA.Contracts.Services;
using Lua_IDEA.Helpers;
using Lua_IDEA.Helpers.Extensions;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;

namespace Lua_IDEA.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    [ObservableProperty]
    private ElementTheme elementTheme;

    [ObservableProperty]
    private string versionDescription;

    [ObservableProperty]
    private bool saveMacroInPmtx;

    [ObservableProperty]
    private bool saveBackInPmtx;

    public ICommand SwitchThemeCommand { get; }

    private readonly IThemeSelectorService themeSelectorService;
    private readonly INavigationService navigationService;

    public SettingsViewModel(
        IThemeSelectorService themeSelectorService,
        INavigationService navigationService)
    {
        this.themeSelectorService = themeSelectorService;
        this.navigationService = navigationService;

        elementTheme = themeSelectorService.Theme;
        versionDescription = GetVersionDescription();

        SwitchThemeCommand = new RelayCommand<ElementTheme>(async (param) =>
        {
            if (ElementTheme != param)
            {
                ElementTheme = param;
                await this.themeSelectorService.SetThemeAsync(param);
            }
        });
    }

    [RelayCommand]
    private void GoBack()
    {
        navigationService.GoBack();
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
