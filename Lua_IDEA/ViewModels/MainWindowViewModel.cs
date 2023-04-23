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
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Lua_IDEA.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<LuaFile> Tabs { get; set; }
    public ObservableCollection<CommandCategory> Categories { get; set; }

    [ObservableProperty]
    public LuaFile selectedTab;

    [ObservableProperty]
    private bool isMacrosPanelVisible;

    public IRelayCommand AddFileCommand { get; set; }
    public IRelayCommand OpenFileCommand { get; set; }
    public IRelayCommand SaveFileCommand { get; set; }
    public IRelayCommand SaveFileAsCommand { get; set; }
    public IRelayCommand SaveAllFileCommand { get; set; }
    public IRelayCommand CloseFileCommand { get; set; }
    public IRelayCommand LoadCommandsCommand { get; set; }
    public IRelayCommand CloseMacroPanelCommand { get; set; }
    
    public IRelayCommand<string> PasteCommand { get; set; }

    private readonly CommandService commandService;

    public MainWindowViewModel()
    {
        commandService = new CommandService();

        Tabs = new ObservableCollection<LuaFile>();
        Categories = new ObservableCollection<CommandCategory>();

        AddFileCommand = new RelayCommand(CreateNewFile);
        OpenFileCommand = new AsyncRelayCommand(OpenFile);
        SaveFileCommand = new AsyncRelayCommand<LuaFile>(SaveFile);
        SaveFileAsCommand = new AsyncRelayCommand(SaveFileAs);
        SaveAllFileCommand = new AsyncRelayCommand(SaveAllFiles);
        CloseFileCommand = new AsyncRelayCommand(CloseFile);
        CloseMacroPanelCommand = new RelayCommand(() => IsMacrosPanelVisible = false);
        LoadCommandsCommand = new AsyncRelayCommand(LoadCommands);
        PasteCommand = new RelayCommand<string>(Paste);

        CreateNewFile();
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
        if (luafile.IsSaved) return;

        if (luafile is null && SelectedTab is not null)
            SaveDialog(SelectedTab);

        if (luafile is not null && SelectedTab is  null)
            SaveDialog(luafile);
    }
    private async Task SaveDialog(LuaFile luaFile)
    {
        var savePicker = new FileSavePicker();

        var hWnd = WindowNative.GetWindowHandle(App.Current);
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
            IsSaved = false
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

            dialog.XamlRoot = (App.Current as MainWindow).Content.XamlRoot;
            dialog.Title = "Save your work?";
            dialog.PrimaryButtonText = "Save";
            dialog.SecondaryButtonText = "Don't Save";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;

            await dialog.ShowAsync();
        }

        Tabs.Remove(SelectedTab);
    }

    private void Paste(string commandText)
    {
        if (SelectedTab is null)
            return;

        SelectedTab.Content += commandText;
    }

    private async Task LoadCommands()
    {
        Categories.Clear();

        var result = await commandService.LoadCommands();

        foreach (var command in result)
        {
            Categories.Add(command);
        }
    }

    private async Task OpenFile()
    {
        var openPicker = new FileOpenPicker();

        var hWnd = WindowNative.GetWindowHandle(App.Current);
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
