using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;

namespace Lua_IDEA.Models;

public partial class LuaFile : ObservableObject
{
    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string path;

    [ObservableProperty]
    private string content;

    [ObservableProperty]
    private bool isSaved;

    [ObservableProperty]
    private bool isFavorite;

    [ObservableProperty]
    private string errors;

    public Visibility HasErrors()
    {
        return Errors?.Length > 0
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
}
