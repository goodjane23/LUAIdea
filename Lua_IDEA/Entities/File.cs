using CommunityToolkit.Mvvm.ComponentModel;

namespace Lua_IDEA.Entities;

public partial class File : ObservableObject
{
    [ObservableProperty]
    public string name;

    [ObservableProperty]
    public string path;

    [ObservableProperty]
    public string content;

    [ObservableProperty]
    public bool isSaved;
}
