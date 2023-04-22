using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lua_IDEA.Entities;
using Lua_IDEA.Services;
using Lua_IDEA.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using WinRT.Interop;

namespace Lua_IDEA.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<LuaFile> Tabs { get; set; }
    public ObservableCollection<CommandCategory> Categories { get; set; }

    [ObservableProperty]
    public LuaFile selectedTab;

    [ObservableProperty]
    public Command selectedCommand;

    public IRelayCommand AddFileCommand { get; set; }
    public IRelayCommand OpenFileCommand { get; set; }
    public IRelayCommand SaveFileCommand { get; set; }
    public IRelayCommand SaveAllFileCommand { get; set; }    
    public IRelayCommand<LuaFile> CloseFileCommand { get; set; }
    public IRelayCommand LoadCommandsCommand { get; set; }
    public IRelayCommand PasteCommand { get; set; }

    private readonly CommandService commandService;

    public MainWindowViewModel()
    {
        commandService = new CommandService();

        Tabs = new ObservableCollection<LuaFile>();
        Categories = new ObservableCollection<CommandCategory>();

        AddFileCommand = new RelayCommand(CreateNewFile);
        OpenFileCommand = new AsyncRelayCommand(OpenFile);
        SaveFileCommand = new AsyncRelayCommand(SaveFile);
        SaveAllFileCommand = new AsyncRelayCommand(SaveAllFiles);
        CloseFileCommand = new AsyncRelayCommand<LuaFile>(CloseFile);
        LoadCommandsCommand = new AsyncRelayCommand(LoadCommands);
        PasteCommand = new RelayCommand(Paste);
    }

    private async Task SaveAllFiles()
    {
        foreach (var tab in Tabs)
            await SaveFile();
    }

    private async Task SaveFile()
    {
        if (!String.IsNullOrWhiteSpace(selectedTab.Path))
        {
            if (!selectedTab.IsSaved)
            {
                var dialog = new ContentDialog();

                dialog.XamlRoot = (App.Current as MainWindow).Content.XamlRoot;
                dialog.Title = "Save your work?";
                dialog.PrimaryButtonText = "Save";
                dialog.SecondaryButtonText = "Don't Save";
                dialog.CloseButtonText = "Cancel";
                dialog.DefaultButton = ContentDialogButton.Primary;

                var result = await dialog.ShowAsync();                
            }
            else
            {
                return;
            }
        }
        else
        {
            FileSavePicker savePicker = new FileSavePicker();
            var hWnd = WindowNative.GetWindowHandle(App.Current);
            InitializeWithWindow.Initialize(savePicker, hWnd);

            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".pm", ".bo", ".txt" });

            savePicker.SuggestedFileName = "m";
            StorageFile storageFile = await savePicker.PickSaveFileAsync();
            if (storageFile is not null)
            {
                CachedFileManager.DeferUpdates(storageFile);
                using (var stream = await storageFile.OpenStreamForWriteAsync())
                {
                    using (var tw = new System.IO.StreamWriter(stream))
                    {
                        tw.WriteLine(selectedTab.content);
                    }
                }
                selectedTab.path = storageFile.Path;
                selectedTab.name = storageFile.Name;
            }
        }      
    }

    private void CreateNewFile()
    {
        Tabs.Add(new LuaFile()
        {
            Name = "New file",
            Path = "",
            Content = "",
            IsSaved = false
        });
    }

    private async Task CloseFile(Entities.LuaFile file)
    {
        if (file is null)
            return;

        if (!file.IsSaved)
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

        Tabs.Remove(file);
    }

    private void Paste()
    {
        if (SelectedCommand is null)
            return;

        if (SelectedTab is null)
            return;

        SelectedTab.Content += SelectedCommand.Name;
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
