using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lua_IDEA.Data.Entities;
using Lua_IDEA.Messages;
using Lua_IDEA.Services;
using System.Collections.ObjectModel;

namespace Lua_IDEA.ViewModels;

public partial class FavoriteFileSelectorViewModel : ObservableObject
{
    public ObservableCollection<FavoriteFile> FavoritePaths { get; } = new();

    [ObservableProperty]
    private FavoriteFile selectedPath;

    private readonly FilesService filesService;

    public FavoriteFileSelectorViewModel(FilesService filesService)
	{
        this.filesService = filesService;
    }

    [RelayCommand]
    private async Task GetFavoritesFiles()
    {
        FavoritePaths.Clear();

        var files = await filesService.GetFavoriteMacros();

        foreach (var file in files)
            FavoritePaths.Add(file);
    }

    [RelayCommand]
    private async Task RemoveFile(string path)
    {
        await filesService.RemoveFromFavorite(path);

        var item = FavoritePaths.FirstOrDefault(x => x.Path == path);

        if (item is not null)
            FavoritePaths.Remove(item);
    }

    [RelayCommand]
    private async Task SelectFile()
    {
        if (string.IsNullOrEmpty(SelectedPath.Path))
            return;

        if (File.Exists(SelectedPath.Path)) 
            WeakReferenceMessenger.Default.Send(new SelectFavoriteFileMessage(SelectedPath.Path));
    }
}
