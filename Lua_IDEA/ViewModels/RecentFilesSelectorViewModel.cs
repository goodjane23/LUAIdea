using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lua_IDEA.Contracts.Services;
using Lua_IDEA.Data.Entities;
using Lua_IDEA.Messages;
using Lua_IDEA.Services;
using System.Collections.ObjectModel;

namespace Lua_IDEA.ViewModels;

public partial class RecentFilesSelectorViewModel : ObservableObject
{
    public event Action FileIsNotExistEvent;

    public ObservableCollection<RecentFile> RecentPaths { get; } = new();

    [ObservableProperty]
    private RecentFile selectedPath;
    private readonly FilesService filesService;

    public RecentFilesSelectorViewModel(
        FilesService filesService)
    {
        this.filesService = filesService;
    }

    [RelayCommand]
    private async Task GetRecentPaths()
    {
        RecentPaths.Clear();
        
        var paths = await filesService.GetRecentMacros();

        foreach (var path in paths)
            RecentPaths.Add(path);
    }

    [RelayCommand]
    private async Task SelectFile()
    {
        if (SelectedPath == null || (string.IsNullOrEmpty(SelectedPath.Path)))
            return;

        if (!File.Exists(SelectedPath.Path))
        {
            FileIsNotExistEvent.Invoke();
            await filesService.RemoveFromRecent(SelectedPath.Path);
            return;
        }

        WeakReferenceMessenger.Default.Send(new SelectRecentFileMessage(SelectedPath.Path));
    }
}
