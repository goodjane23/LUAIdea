using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lua_IDEA.Data.Entities;
using Lua_IDEA.Models;
using Lua_IDEA.Services;
using Lua_IDEA.Views;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Lua_IDEA.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public event Action CommandPasted;
    public event Func<LuaFile, Task> SaveRequested;
    public event Func<LuaFile, Task<bool>> CloseRequested;

    public ObservableCollection<LuaFile> Tabs { get; } = new();
    public ObservableCollection<string> FavoritesMacros { get; set; } = new();
    public ObservableCollection<CommandCategory> Macros { get; } = new();
    public ObservableCollection<CommandCategory> BackgroudOperations { get; } = new();

    [ObservableProperty]
    public LuaFile selectedTab;

    [ObservableProperty]
    private bool isMacrosPanelVisible;

    [ObservableProperty]
    private Command selectedCommand;

    private readonly CommandService commandService;
    private readonly SyntaxCheckerService syntaxChecker;

    public MainWindowViewModel()
    {
        commandService = new CommandService();
        syntaxChecker = new SyntaxCheckerService();
        
        CreateNewFile();
    }

    [RelayCommand]
    private async Task SaveFileAs()
    {
        if (SelectedTab is null)
            return;

        await SaveRequested.Invoke(SelectedTab);
    }

    [RelayCommand]
    private async Task SaveAllFiles()
    {
        throw new NotImplementedException();
    }

    [RelayCommand]
    private async Task SaveFile(LuaFile luafile)
    {
        luafile ??= SelectedTab;

        if (luafile is null || luafile.IsSaved)
            return;

        await SaveRequested.Invoke(luafile);
    }

    [RelayCommand]
    private void CreateNewFile()
    {
        var file = new LuaFile()
        {
            Name = "New file",
            Path = "",
            Content = "",
            IsSaved = false,
            IsFavorite = false,
        };

        Tabs.Add(file);
    }

    [RelayCommand]
    private async Task CloseFile(LuaFile? file)
    {
        file ??= SelectedTab;

        if (file is null)
            return;

        if (file.IsSaved)
        {
            Tabs.Remove(file);
            return;
        }

        var result = await CloseRequested.Invoke(file);
        
        if (result)
        {
            Tabs.Remove(file);
            return;
        }

        if (!result && !file.IsSaved)
            await SaveRequested.Invoke(file);
    }

    [RelayCommand]
    private void Paste()
    {
        if (SelectedTab is null)
            return;

        if (SelectedCommand is not Command command)
            return;

        SelectedTab.Content += $"{command.Name}\n";
        SelectedTab.IsSaved = false;
        SelectedTab.Errors = syntaxChecker.CheckSyntax(SelectedTab.Content);

        CommandPasted?.Invoke();
    }

    [RelayCommand]
    private async Task UpdateCommands()
    {
        Macros.Clear();
        BackgroudOperations.Clear();

        SelectedCommand = null!;

        var result = await commandService.LoadCommands();

        foreach (var command in result.Where(x => x.IsMacro))
            Macros.Add(command);

        foreach (var command in result.Where(x => !x.IsMacro))
            BackgroudOperations.Add(command);
    }

    [RelayCommand]
    private async Task OpenFile()
    {
        var openPicker = new FileOpenPicker();

        var hWnd = WindowNative.GetWindowHandle(App.MainWindow);
        InitializeWithWindow.Initialize(openPicker, hWnd);

        openPicker.ViewMode = PickerViewMode.Thumbnail;
        openPicker.SuggestedStartLocation = PickerLocationId.Desktop;

        openPicker.FileTypeFilter.Add(".pm");
        openPicker.FileTypeFilter.Add(".txt");
        openPicker.FileTypeFilter.Add(".bo");

        var storageFile = await openPicker.PickSingleFileAsync();

        if (storageFile is null) return;
     
        var fileContent = await FileIO.ReadTextAsync(storageFile);

        var file = new LuaFile
        {
            Name = storageFile.Name,
            Path = storageFile.Path,
            Content = fileContent,
            IsSaved = true
        };

        Tabs.Add(file);
    }

    private async Task GetFavoritesMacrosAsync()
    {
       var result = await FileService.GetFavoriteMacros();
        foreach (var favoriteMacro in result)
        {
            FavoritesMacros.Add(favoriteMacro); 
        }
    }

    [RelayCommand]
    private async void ChangeFavoriteStatus()
    {
        if (SelectedTab is null) return;

        if (String.IsNullOrEmpty(SelectedTab.Path) || SelectedTab.Path=="")
            await SaveFile(SelectedTab);
      

        if (SelectedTab.IsFavorite)
        {
            var temp = await FileService.AddToFavorite(SelectedTab.Path);
        }
        else
        {
            var temp = await FileService.RemoveFromFavorite(SelectedTab.Path);
        }
    }
}
