using Lua_IDEA.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Lua_IDEA.Views.Pages;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();

        InitializeComponent();
    }
}
