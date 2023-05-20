using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lua_IDEA.Messages;
using Lua_IDEA.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Lua_IDEA.ViewModels;

public partial class RecentFilesSelectorViewModel : ObservableObject
{
    public ObservableCollection<string> RecentPaths { get; } = new();

    [ObservableProperty]
    private string selectedPath;

    private readonly FilesServise filesService;

    public RecentFilesSelectorViewModel(FilesServise filesService)
    {
        this.filesService = filesService;
    }

    [RelayCommand]
    private async Task GetRecentPaths()
    {
        var paths = await filesService.GetRecentMacros();

        foreach (var path in paths)
            RecentPaths.Add(path);
    }

    [RelayCommand]
    private void SelectFile()
    {
        WeakReferenceMessenger.Default.Send(new SelectRecentFileMessage(SelectedPath));
    }
}
