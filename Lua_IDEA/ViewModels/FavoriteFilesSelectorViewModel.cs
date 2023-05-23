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

namespace Lua_IDEA.ViewModels;
public partial class FavoriteFilesSelectorViewModel : ObservableObject
{
    private readonly FilesServise filesServise;

    public ObservableCollection<string> FavoritePaths { get; set; } = new();

    [ObservableProperty]
    private string selectedPath;

    public FavoriteFilesSelectorViewModel(FilesServise filesService)
    {
        this.filesServise = filesService;
    }

    [RelayCommand]
    private async Task GetFavoritePaths()
    {
        var paths = await filesServise.GetFavoriteMacros();

        foreach (var path in paths)
            FavoritePaths.Add(path);
    }

    [RelayCommand]
    private void SelectFile()
    {
        WeakReferenceMessenger.Default.Send(new SelectRecentFileMessage(SelectedPath));
    }
}
