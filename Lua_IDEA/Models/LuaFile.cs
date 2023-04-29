using CommunityToolkit.Mvvm.ComponentModel;

namespace Lua_IDEA.Models;

public partial class LuaFile : ObservableObject
{
    [ObservableProperty]
    public string name;

    [ObservableProperty]
    public string path;

    [ObservableProperty]
    public string content;

    [ObservableProperty]
    public bool isSaved;

    [ObservableProperty]
    public bool isFavorite;
}
