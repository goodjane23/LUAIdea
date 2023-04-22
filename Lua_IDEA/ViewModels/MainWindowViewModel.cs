using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lua_IDEA.Entities;
using Lua_IDEA.Services;
using Lua_IDEA.Views;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Lua_IDEA.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<File> Tabs { get; set; }
    public ObservableCollection<CommandCategory> Categories { get; set; }

    [ObservableProperty]
    public File selectedTab;

    [ObservableProperty]
    public Command selectedCommand;

    public IRelayCommand AddFileCommand { get; set; }
    public IRelayCommand OpenFileCommand { get; set; }
    public IRelayCommand<File> CloseFileCommand { get; set; }
    public IRelayCommand LoadCommandsCommand { get; set; }
    public IRelayCommand PasteCommand { get; set; }

    private readonly CommandService commandService;

    public MainWindowViewModel()
    {
        commandService = new CommandService();

        Tabs = new ObservableCollection<File>();
        Categories = new ObservableCollection<CommandCategory>();

        AddFileCommand = new RelayCommand(CreateNewFile);
        OpenFileCommand = new AsyncRelayCommand(OpenFile);
        CloseFileCommand = new AsyncRelayCommand<File>(CloseFile);
        LoadCommandsCommand = new AsyncRelayCommand(LoadCommands);
        PasteCommand = new RelayCommand(Paste);
    }

    private void CreateNewFile()
    {
        Tabs.Add(new File()
        {
            Name = "New file",
            Path = "",
            Content = "",
            IsSaved = false
        });
    }

    private async Task CloseFile(File file)
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

        var file = new File
        {
            Name = storageFile.Name,
            Path = storageFile.Path,
            Content = fileContent,
            IsSaved = true
        };

        Tabs.Add(file);
    }
}
