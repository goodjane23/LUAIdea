using Lua_IDEA.Data.Entities;
using Lua_IDEA.Factory;
using Lua_IDEA.Models;
using Lua_IDEA.ViewModels;
using Lua_IDEA.Views.Dialogs;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using WinUIEx;

namespace Lua_IDEA.Views;

public sealed partial class MainWindow : WindowEx
{
    private RichEditBox currentTextEditor;

    private readonly MainWindowViewModel viewModel;
    private readonly WindowFactory<RecentFilesDialogSelector> recentWindow;
    private readonly WindowFactory<RecentFilesDialogSelector> favoritesWindow;

    public MainWindow(
        MainWindowViewModel viewModel,
        WindowFactory<RecentFilesDialogSelector> windowFactory,
        WindowFactory<RecentFilesDialogSelector> favoritesWindow)
    {
        InitializeComponent();

        this.recentWindow = windowFactory;
        this.favoritesWindow = favoritesWindow;

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(titleBar);

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/appicon.ico"));

        this.viewModel = viewModel;

        viewModel.CommandPasted += OnCommandPasted;
        viewModel.SaveRequested += OnSaveRequested;
        viewModel.CloseRequested += OnCloseRequested;
        viewModel.SaveCheckRequested += OnSaveCheckRequested;

        SizeChanged += (sender, args) =>
        {
            if (currentTextEditor is not null)
                currentTextEditor.Height = tabViewGrid.ActualSize.ToSize().Height;
        };
        this.favoritesWindow = favoritesWindow;
    }

    private async Task<bool> OnSaveCheckRequested()
    {
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = $"Необходимо сохранить файл, перед тем как добавить файл в избранное. Сохранть?",
            PrimaryButtonText = "Сохранить",
            SecondaryButtonText = "Не сохранять",
            CloseButtonText = "Отмена",
            DefaultButton = ContentDialogButton.Primary
        };

        var dialogResult = await dialog.ShowAsync();

        return dialogResult == ContentDialogResult.Primary;
    }

    private void OnCommandPasted()
    {
        currentTextEditor?.Document.SetText(TextSetOptions.None, viewModel.SelectedTab?.Content);
    }

    private async Task<bool> OnCloseRequested(LuaFile file)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = $"Сохранить изменения в файле {file.Name}?",
            PrimaryButtonText = "Сохранить",
            SecondaryButtonText = "Не сохранять",
            CloseButtonText = "Отмена",
            DefaultButton = ContentDialogButton.Primary
        };

        var dialogResult = await dialog.ShowAsync();

        return dialogResult == ContentDialogResult.Secondary;
    }


    private async Task OnSaveRequested(LuaFile file)
    {
        var savePicker = new FileSavePicker();

        var hWnd = WindowNative.GetWindowHandle(this);
        InitializeWithWindow.Initialize(savePicker, hWnd);

        savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        savePicker.FileTypeChoices.Add("Macro File", new List<string>() { ".pm" });
        savePicker.FileTypeChoices.Add("Background Operation", new List<string>() { ".bo" });
        savePicker.FileTypeChoices.Add("Text File", new List<string>() { ".txt" });

        savePicker.SuggestedFileName = file.Name;

        var storageFile = await savePicker.PickSaveFileAsync();

        if (storageFile is null)
            return;

        CachedFileManager.DeferUpdates(storageFile);

        using var stream = await storageFile.OpenStreamForWriteAsync();
        using var tw = new StreamWriter(stream);

        tw.WriteLine(file.Content);

        file.Path = storageFile.Path;
        file.Name = storageFile.Name;
        file.IsSaved = true;
    }

    private void TreeViewItem_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        viewModel.PasteCommand.Execute(null);
    }

    private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        viewModel.CloseFileCommand.Execute(args.Item as LuaFile);
    }

    private void Grid_Loaded(object sender, RoutedEventArgs e)
    {
        viewModel.UpdateCommandsCommand.Execute(null);
    }

    private void TreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
    {
        if (args.InvokedItem is not Command command)
            return;

        viewModel.SelectedCommand = command;
    }

    private void textEditor_TextChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not RichEditBox textEditor)
            return;

        var text = string.Empty;
        textEditor.TextDocument.GetText(TextGetOptions.UseObjectText, out text);

        viewModel.SelectedTab.Content = text;
        viewModel.SelectedTab.IsSaved = false;
    }

    private void textEditor_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not RichEditBox textEditor)
            return;

        textEditor.Height = tabViewGrid.ActualSize.ToSize().Height;
        currentTextEditor = textEditor;

        var text = viewModel.SelectedTab?.Content;

        textEditor?.Document.SetText(TextSetOptions.None, text);
    }

    private async void ShowRecentDialog(object sender, RoutedEventArgs e)
    {
        var resentDialog = recentWindow.Create();
        resentDialog.XamlRoot = this.Content.XamlRoot;

        resentDialog.Title = "Выберите файл";
        resentDialog.PrimaryButtonText = "Открыть";
        resentDialog.CloseButtonText = "Отмена";
        resentDialog.DefaultButton = ContentDialogButton.Primary;
        await resentDialog.ShowAsync();

    }

    private async void ShowFavoriteDialog(object sender, RoutedEventArgs e)
    {
        var favoritesDialog = favoritesWindow.Create();
        favoritesDialog.XamlRoot = this.Content.XamlRoot;

        favoritesDialog.Title = "Выберите файл";
        favoritesDialog.PrimaryButtonText = "Открыть";
        favoritesDialog.CloseButtonText = "Отмена";
        favoritesDialog.DefaultButton = ContentDialogButton.Primary;
        await favoritesDialog.ShowAsync();
    }
}