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
    [NotifyPropertyChangedFor(nameof(IsChangedIconVisible))]   
    private bool isSaved;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasErrors))]
    private string errors;

    public bool IsChangedIconVisible => !IsSaved;

    public Visibility HasErrors
    {
        get
        {
            return Errors?.Length > 0
            ? Visibility.Visible
            : Visibility.Collapsed;
        }
    }
}
