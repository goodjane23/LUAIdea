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
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Lua_IDEA.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<LuaFile> Tabs { get; }
    public ObservableCollection<CommandCategory> Categories { get; }
    public ObservableCollection<LuaFile> FavoritesMacros { get; }

    [ObservableProperty]
    public LuaFile selectedTab;

    [ObservableProperty]
    private bool isMacrosPanelVisible;

    [ObservableProperty]
    private object selectedCommand;

    [ObservableProperty]
    private string errorText;

    public IRelayCommand AddFileCommand { get; }
    public IRelayCommand OpenFileCommand { get; }
    public IRelayCommand SaveFileCommand { get; }
    public IRelayCommand SaveFileAsCommand { get; }
    public IRelayCommand SaveAllFileCommand { get; }
    public IRelayCommand CloseFileCommand { get; }
    public IRelayCommand LoadCommandsCommand { get; }
    public IRelayCommand CloseMacroPanelCommand { get; }
    public IRelayCommand PasteCommand { get; }
    public IRelayCommand AddToFavoritesCommand { get; }
    public IRelayCommand RemoveToFavoritesCommand { get; }

    public IRelayCommand TextChangingCommand { get; }

    private readonly CommandService commandService;

    public MainWindowViewModel()
    {
        commandService = new CommandService();

        Tabs = new ObservableCollection<LuaFile>();
        Categories = new ObservableCollection<CommandCategory>();
        FavoritesMacros = new ObservableCollection<LuaFile>();

        AddFileCommand = new RelayCommand(CreateNewFile);
        OpenFileCommand = new AsyncRelayCommand(OpenFile);
        SaveFileCommand = new AsyncRelayCommand<LuaFile>(SaveFile);
        SaveFileAsCommand = new AsyncRelayCommand(SaveFileAs);
        SaveAllFileCommand = new AsyncRelayCommand(SaveAllFiles);
        CloseFileCommand = new AsyncRelayCommand(CloseFile);
        TextChangingCommand = new RelayCommand(TextChanging);
        CloseMacroPanelCommand = new RelayCommand(() => IsMacrosPanelVisible = false);
        LoadCommandsCommand = new AsyncRelayCommand(LoadCommands);
        AddToFavoritesCommand = new RelayCommand(AddToFavorites);
        RemoveToFavoritesCommand = new RelayCommand(RemoveToFavorites);
        PasteCommand = new RelayCommand(Paste);

        CreateNewFile();
    }

    private void TextChanging()
    {
        ErrorText = SyntaxCheckService.SyntaxCheck(SelectedTab.Content);    
        
    }

    private async Task SaveFileAs()
    {
        await SaveDialog(SelectedTab);
    }

    private async Task SaveAllFiles()
    {
        foreach (var tab in Tabs)
            await SaveFile(tab);
    }
    
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

    private void CreateNewFile()
    {
        var file = new LuaFile()
        {
            Name = "New file",
            Path = "",
            Content = "",
            IsSaved = false,
            isFavorite = false,
        };

        Tabs.Add(file);
    }

    private async Task CloseFile()
    {
        if (SelectedTab is null)
            return;

        if (!SelectedTab.IsSaved)
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
        
        Tabs.Remove(SelectedTab);
    }

    private void Paste()
    {
        if (SelectedTab is null)
            return;

        if (SelectedCommand is not Command command)
            return;

        SelectedTab.Content += $"{command.Name}\n";
    }

    private async Task LoadCommands()
    {
        var result = await commandService.LoadCommands();

        foreach (var command in result)
        {
            Categories.Add(command);
        }
    }

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

    private async void AddToFavorites()
    {
        if (SelectedTab.Path is null)
            await SaveFile(SelectedTab);
        SelectedTab.IsFavorite = true;
        FavoritesMacros.Add(SelectedTab);
    }

    private void RemoveToFavorites()
    {
        FavoritesMacros.Remove(SelectedTab);
    }
}
