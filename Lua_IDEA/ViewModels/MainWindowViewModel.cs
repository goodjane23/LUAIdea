using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lua_IDEA.Data.Entities;
using Lua_IDEA.Models;
using Lua_IDEA.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using WinRT.Interop;
using Lua_IDEA.Views;
using WinUIEx;
using Lua_IDEA.Factory;
using Lua_IDEA.Views.Dialogs;

namespace Lua_IDEA.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public event Action CommandPasted;
    public event Func<LuaFile, Task> SaveRequested;
    public event Func<Task<bool>> SaveCheckRequested;
    public event Func<LuaFile, Task<bool>> CloseRequested;

    private readonly CommandService commandService;
    private readonly SyntaxChecker syntaxChecker;
    private readonly FilesServise filesService;
    private readonly WindowFactory<RecentFilesDialogSelector> recentDialogFactory;

    [ObservableProperty]
    private LuaFile selectedTab;

    [ObservableProperty]
    private bool isMacrosPanelVisible;

    [ObservableProperty]
    private Command selectedCommand;

    public ObservableCollection<LuaFile> Tabs { get; } = new();
    public ObservableCollection<string> FavoritesMacros { get; set; } = new();
    public ObservableCollection<string> RecentMacros { get; set; } = new();
    public ObservableCollection<CommandCategory> Macros { get; } = new();
    public ObservableCollection<CommandCategory> BackgroudOperations { get; } = new();

    public MainWindowViewModel(
        CommandService commandService,
        SyntaxChecker syntaxChecker,
        FilesServise filesService,
        WindowFactory<RecentFilesDialogSelector> windowFactory)
    {
        this.commandService = commandService;
        this.syntaxChecker = syntaxChecker;
        this.filesService = filesService;
        this.recentDialogFactory = windowFactory;
        RecentMacros.Add("werty");
        RecentMacros.Add("werty");
        RecentMacros.Add("werty");
        CreateNewFile();
    }

    [RelayCommand]
    private async Task SaveFileAs()
    {
        if (SelectedTab is null)
            return;

        await SaveRequested.Invoke(SelectedTab);
        await filesService.AddToRecent(SelectedTab.Path);
    }

    [RelayCommand]
    private async Task SaveAllFiles()
    {
        foreach (var tab in Tabs)
        {
            if (!tab.IsSaved)
            {
                await SaveFile(tab);
            }
        }
    }

    [RelayCommand]
    private async Task SaveFile(LuaFile luafile)
    {
        luafile ??= SelectedTab;

        if (luafile is null || luafile.IsSaved)
            return;

        await SaveRequested.Invoke(luafile);
        await filesService.AddToFavorite(SelectedTab.Path);
    }

    [RelayCommand]
    private void CreateNewFile()
    {
        var file = new LuaFile()
        {
            Name = "M",
            Path = "",
            Content = "function M() \r\n\r\n end",
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
        var result = await filesService.GetFavoriteMacros();

        foreach (var favoriteMacro in result)
            FavoritesMacros.Add(favoriteMacro);
    }

    [RelayCommand]
    private async Task ChangeFavoriteStatus()
    {
        bool res = true;

        if (SelectedTab is null)
            return;

        if (String.IsNullOrEmpty(SelectedTab.Path))
            res = await SaveCheckRequested.Invoke();

        if (!res) return;
       
        await SaveFile(SelectedTab);

        if (SelectedTab.IsFavorite)
            await filesService.RemoveFromFavorite(SelectedTab.Path);       
        else
            await filesService.AddToFavorite(SelectedTab.Path);

        var result = await filesService.GetFavoriteMacros();

        FavoritesMacros.Clear();

        foreach (var favoriteMacro in result)
            FavoritesMacros.Add(favoriteMacro);

        SelectedTab.IsFavorite = !SelectedTab.IsFavorite;
    }

    [RelayCommand]
    private async Task RecentDialogShow()
    {
        var recentdialog = recentDialogFactory.Create();
        await recentdialog.ShowAsync();
        
    }

}
