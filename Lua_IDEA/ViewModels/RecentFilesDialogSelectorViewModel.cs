using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lua_IDEA.Messages;
using Lua_IDEA.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.VoiceCommands;

namespace Lua_IDEA.ViewModels;
public partial class RecentFilesDialogSelectorViewModel : ObservableObject
{
    private readonly FilesServise filesServise;

    public ObservableCollection<string> RecentPaths { get; set; } = new();

    [ObservableProperty]
    private string selectedPath;

    public RecentFilesDialogSelectorViewModel(FilesServise filesService)
    {
        this.filesServise = filesService;
    }

    [RelayCommand]
    private async Task GetRecentPaths()
    {
        var paths = await filesServise.GetRecentMacros();

        foreach (var path in paths)
        {
            RecentPaths.Add(path);
        }

    }

    [RelayCommand]
    private void SelectFile()
    {
        WeakReferenceMessenger.Default.Send(new SelectRecentFileMessage(SelectedPath));
    }
}
