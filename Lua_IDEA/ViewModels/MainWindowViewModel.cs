using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lua_IDEA.Entities;
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
    public ObservableCollection<LuaFile> Tabs { get; } = new();
    public ObservableCollection<LuaFile> FavoritesMacros { get; } = new();
    public ObservableCollection<CommandCategory> Macros { get; } = new();
    public ObservableCollection<CommandCategory> BackgroudOperations { get; } = new();

    [ObservableProperty]
    public LuaFile selectedTab;

    [ObservableProperty]
    private bool isMacrosPanelVisible;

    [ObservableProperty]
    private object selectedCommand;

    [ObservableProperty]
    private string errorText;

    private readonly CommandService commandService;
    private readonly SyntaxCheckerService syntaxChecker;

    public MainWindowViewModel()
    {
        commandService = new CommandService();
        syntaxChecker = new SyntaxCheckerService();

        CreateNewFile();
    }

    [RelayCommand]
    private void CloseMacroPanel()
    {
        IsMacrosPanelVisible = false;
    }

    [RelayCommand]
    private void TextChanging()
    {
        ErrorText = syntaxChecker.CheckSyntax(SelectedTab.Content);    
    }

    [RelayCommand]
    private async Task SaveFileAs()
    {
        await SaveDialog(SelectedTab);
    }

    [RelayCommand]
    private async Task SaveAllFiles()
    {
        foreach (var tab in Tabs)
            await SaveFile(tab);
    }

    [RelayCommand]
    private async Task SaveFile(LuaFile luafile)
    {
        if (luafile is null && SelectedTab is not null)
            await SaveDialog(SelectedTab);

        if (luafile is not null && SelectedTab is  null)
            await SaveDialog(luafile);
    }

    private async Task SaveDialog(LuaFile luaFile)
    {
        var savePicker = new FileSavePicker();

        var hWnd = WindowNative.GetWindowHandle(App.MainWindow);
        InitializeWithWindow.Initialize(savePicker, hWnd);

        savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".pm", ".bo", ".txt" });

        savePicker.SuggestedFileName = "file";

        var storageFile = await savePicker.PickSaveFileAsync();

        if (storageFile is not null)
        {
            CachedFileManager.DeferUpdates(storageFile);
            using var stream = await storageFile.OpenStreamForWriteAsync();

            using (var tw = new StreamWriter(stream))
            {
                tw.WriteLine(luaFile.Content);
            }

            luaFile.Path = storageFile.Path;
            luaFile.Name = storageFile.Name;
            luaFile.IsSaved = true;
        }
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

        if (!file.IsSaved)
        {
            var dialog = new ContentDialog();

            dialog.XamlRoot = (App.MainWindow as MainWindow).Content.XamlRoot;
            dialog.Title = "Save your work?";
            dialog.PrimaryButtonText = "Save";
            dialog.SecondaryButtonText = "Don't Save";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;

            await dialog.ShowAsync();
        }
        
        Tabs.Remove(file);
    }

    [RelayCommand]
    private void Paste()
    {
        if (SelectedTab is null)
            return;

        if (SelectedCommand is not Command command)
            return;

        SelectedTab.Content += $"{command.Name}\n";
    }

    [RelayCommand]
    private async Task UpdateCommands()
    {
        Macros.Clear();
        BackgroudOperations.Clear();

        var result = await commandService.LoadCommands();

        foreach (var command in result.Where(x => x.Name.StartsWith('5')))
            Macros.Add(command);

        foreach (var command in result.Where(x => x.Name.StartsWith('4')))
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

        openPicker.FileTypeFilter.Add(".txt");

        var storageFile = await openPicker.PickSingleFileAsync();
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
}
