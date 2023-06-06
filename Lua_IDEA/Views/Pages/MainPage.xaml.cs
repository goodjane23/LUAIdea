using Lua_IDEA.Contracts.Services;
using Lua_IDEA.Data.Entities;
using Lua_IDEA.Factories;
using Lua_IDEA.Helpers;
using Lua_IDEA.Models;
using Lua_IDEA.ViewModels;
using Lua_IDEA.Views.Dialogs;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Numerics;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.System;
using WinRT.Interop;

namespace Lua_IDEA.Views.Pages;

public sealed partial class MainPage : Page
{
    public MainPageViewModel ViewModel { get; }

    private RichEditBox currentTextEditor;

    private readonly DialogFactory<RecentFilesSelector> recentDialogFactory;
    private readonly DialogFactory<FavoriteFileSelector> favoriteDialogFactory;
    private readonly IThemeSelectorService themeSelectorService;

    public MainPage()
    {
        ViewModel = App.GetService<MainPageViewModel>();
        recentDialogFactory = App.GetService<DialogFactory<RecentFilesSelector>>();
        favoriteDialogFactory = App.GetService<DialogFactory<FavoriteFileSelector>>();
        themeSelectorService = App.GetService<IThemeSelectorService>();

        InitializeComponent();

        ViewModel.CommandPasted += OnCommandPasted;
        ViewModel.SaveRequested += OnSaveRequested;
        ViewModel.CloseRequested += OnCloseRequested;

        SizeChanged += (sender, args) =>
        {
            if (currentTextEditor is not null)
                currentTextEditor.Height = tabViewGrid.ActualSize.ToSize().Height - 45;
        };
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.UpdateCommandsCommand.Execute(null);

        TitleBarHelper.UpdateTitleBar(themeSelectorService.Theme);

        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));

        ShellMenuBarSettingsButton.AddHandler(UIElement.PointerPressedEvent,
            new PointerEventHandler(ShellMenuBarSettingsButton_PointerPressed), true);
        ShellMenuBarSettingsButton.AddHandler(UIElement.PointerReleasedEvent,
            new PointerEventHandler(ShellMenuBarSettingsButton_PointerReleased), true);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ShellMenuBarSettingsButton.RemoveHandler(UIElement.PointerPressedEvent,
            (PointerEventHandler)ShellMenuBarSettingsButton_PointerPressed);
        ShellMenuBarSettingsButton.RemoveHandler(UIElement.PointerReleasedEvent,
            (PointerEventHandler)ShellMenuBarSettingsButton_PointerReleased);

        ViewModel.CommandPasted -= OnCommandPasted;
        ViewModel.SaveRequested -= OnSaveRequested;
        ViewModel.CloseRequested -= OnCloseRequested;
    }

    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        var keyboardAccelerator = new KeyboardAccelerator() { Key = key };

        if (modifiers.HasValue)
        {
            keyboardAccelerator.Modifiers = modifiers.Value;
        }

        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;

        return keyboardAccelerator;
    }

    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender,
        KeyboardAcceleratorInvokedEventArgs args)
    {
        var navigationService = App.GetService<INavigationService>();
        var result = navigationService.GoBack();

        args.Handled = result;
    }

    private async void ShowRecentDialog(object sender, RoutedEventArgs e)
    {
        var resentDialog = recentDialogFactory.Create();

        resentDialog.XamlRoot = Content.XamlRoot;
        resentDialog.Title = "Недавние файлы";
        resentDialog.PrimaryButtonText = "Открыть";
        resentDialog.CloseButtonText = "Отмена";
        resentDialog.RequestedTheme = themeSelectorService.Theme;
        resentDialog.DefaultButton = ContentDialogButton.Primary;

        await resentDialog.ShowAsync();
    }

    private async void ShowFavoriteDialog(object sender, RoutedEventArgs e)
    {
        var resentDialog = favoriteDialogFactory.Create();

        resentDialog.XamlRoot = Content.XamlRoot;
        resentDialog.Title = "Избранные файлы";
        resentDialog.PrimaryButtonText = "Открыть";
        resentDialog.CloseButtonText = "Отмена";
        resentDialog.RequestedTheme = themeSelectorService.Theme;
        resentDialog.DefaultButton = ContentDialogButton.Primary;

        await resentDialog.ShowAsync();
    }

    private void TreeViewItem_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        ViewModel.PasteCommand.Execute(null);
    }

    private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        ViewModel.CloseFileCommand.Execute(args.Item as LuaFile);
    }

    private void TreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
    {
        if (args.InvokedItem is not Command command)
            return;

        ViewModel.SelectedCommand = command;
    }

    private void textEditor_TextChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not RichEditBox textEditor)
            return;

        textEditor.TextDocument.GetText(TextGetOptions.UseObjectText, out var text);

        ViewModel.SelectedTab.Content = text;
        ViewModel.SelectedTab.IsSaved = false;
    }

    private void textEditor_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not RichEditBox textEditor)
            return;

        textEditor.Height = tabViewGrid.ActualSize.ToSize().Height - 45;
        currentTextEditor = textEditor;

        textEditor.Document.GetText(TextGetOptions.UseObjectText, out var text);

        if (text != ViewModel.SelectedTab.Content)
            textEditor.Document.SetText(TextSetOptions.None, ViewModel.SelectedTab.Content);
    }

    private void OnCommandPasted()
    {
        currentTextEditor?.Document.SetText(TextSetOptions.None, ViewModel.SelectedTab?.Content);
    }

    private async Task<CloseRequestResult> OnCloseRequested(LuaFile file)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = $"Сохранить изменения в файле {file.Name}?",
            PrimaryButtonText = "Сохранить",
            SecondaryButtonText = "Не сохранять",
            CloseButtonText = "Отмена",
            RequestedTheme = themeSelectorService.Theme,
            DefaultButton = ContentDialogButton.Primary
        };

        var dialogResult = await dialog.ShowAsync();

        return dialogResult switch
        {
            ContentDialogResult.Primary => CloseRequestResult.Confirmed,
            ContentDialogResult.Secondary => CloseRequestResult.Rejected,
            ContentDialogResult.None => CloseRequestResult.Canceled
        };
    }

    private async Task<bool> OnSaveRequested(LuaFile file, bool saveAs)
    {
        var savePicker = new FileSavePicker();

        if (!string.IsNullOrEmpty(file.Path) && !saveAs)
        {
            await File.WriteAllTextAsync(file.Path, file.Content);

            file.IsSaved = true;

            return true;
        }

        var hWnd = WindowNative.GetWindowHandle(App.MainWindow);
        InitializeWithWindow.Initialize(savePicker, hWnd);

        savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        savePicker.FileTypeChoices.Add("Macro File", new List<string>() { ".pm" });
        savePicker.FileTypeChoices.Add("Background Operation", new List<string>() { ".bo" });
        savePicker.SuggestedFileName = file.Name;

        var storageFile = await savePicker.PickSaveFileAsync();

        if (storageFile is null)
            return false;
        
        CachedFileManager.DeferUpdates(storageFile);

        await using var stream = await storageFile.OpenStreamForWriteAsync();
        await using var tw = new StreamWriter(stream);

        await tw.WriteLineAsync(file.Content);

        file.Path = storageFile.Path;
        file.Name = storageFile.Name;
        file.IsSaved = true;

        return true;
    }

    private void ShellMenuBarSettingsButton_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        AnimatedIcon.SetState((UIElement)sender, "PointerOver");
    }

    private void ShellMenuBarSettingsButton_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        AnimatedIcon.SetState((UIElement)sender, "Pressed");
    }

    private void ShellMenuBarSettingsButton_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        AnimatedIcon.SetState((UIElement)sender, "Normal");
    }

    private void ShellMenuBarSettingsButton_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        AnimatedIcon.SetState((UIElement)sender, "Normal");
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        this.UpdateLayout();
    }
}