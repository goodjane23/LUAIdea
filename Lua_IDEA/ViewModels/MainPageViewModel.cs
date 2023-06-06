using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lua_IDEA.Data.Entities;
using Lua_IDEA.Models;
using Lua_IDEA.Services;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using CommunityToolkit.Mvvm.Messaging;
using Lua_IDEA.Messages;
using Lua_IDEA.Contracts.Services;

namespace Lua_IDEA.ViewModels;

public partial class MainPageViewModel : ObservableObject, IRecipient<SelectRecentFileMessage>, IRecipient<SelectFavoriteFileMessage>
{
    public event Action CommandPasted;
    public event Func<LuaFile, bool, Task<bool>> SaveRequested;
    public event Func<LuaFile, Task<CloseRequestResult>> CloseRequested;

    [ObservableProperty]
    private LuaFile selectedTab;

    [ObservableProperty]
    private bool isMacrosPanelVisible;

    [ObservableProperty]
    private bool isInnerMacrosPanelVisible;

    [ObservableProperty]
    private Command selectedCommand;

    public ObservableCollection<LuaFile> Tabs { get; } = new();
    public ObservableCollection<CommandCategory> Macros { get; } = new();
    public ObservableCollection<CommandCategory> BackgroundOperations { get; } = new();

    public ObservableCollection<LuaFile> InnerMacros { get; set; } = new();
    public ObservableCollection<LuaFile> InnerBO { get; set; } = new();

    public INavigationService NavigationService { get; }

    private readonly ParsingMacroAPIService commandService;
    private readonly SyntaxChecker syntaxChecker;
    private readonly FilesService filesService;
    private readonly ExistMacroService existMacroService;

    public MainPageViewModel(
        ParsingMacroAPIService commandService,
        SyntaxChecker syntaxChecker,
        FilesService filesService,
        ExistMacroService existMacroService,
        INavigationService navigationService)
    {
        this.commandService = commandService;
        this.syntaxChecker = syntaxChecker;
        this.filesService = filesService;
        this.existMacroService = existMacroService;

        NavigationService = navigationService;

        WeakReferenceMessenger.Default.RegisterAll(this);

        CreateNewFile();
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        NavigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
    }

    [RelayCommand]
    private async Task SaveFileAs()
    {
        if (SelectedTab is null)
            return;

        await SaveRequested.Invoke(SelectedTab, true);
        await filesService.AddToRecent(SelectedTab.Path);
    }

    [RelayCommand]
    private async Task SaveAllFiles()
    {
        foreach (var tab in Tabs)
        {
            if (!tab.IsSaved)
                await SaveFile(tab);
        }
    }

    [RelayCommand]
    private async Task SaveFile(LuaFile luafile)
    {
        luafile ??= SelectedTab;

        if (luafile is null || luafile.IsSaved)
            return;

        await SaveRequested.Invoke(luafile, false);
        await filesService.AddToRecent(SelectedTab.Path);
    }

    [RelayCommand]
    private void CreateNewFile()
    {
        var file = new LuaFile()
        {
            Name = "M",
            Path = "",
            Content = "function M() \r\n\r\n end",
            IsSaved = true,
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

        if (result == CloseRequestResult.Rejected)
        {
            Tabs.Remove(file);
            return;
        }

        if (result == CloseRequestResult.Confirmed && !file.IsSaved)
        {
            var saveResult = await SaveRequested.Invoke(file, false);

            if (saveResult)
                Tabs.Remove(file);
        }
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
        BackgroundOperations.Clear();

        SelectedCommand = null!;

        var result = await commandService.LoadCommands();

        foreach (var command in result.Where(x => x.IsMacro))
            Macros.Add(command);

        foreach (var command in result.Where(x => !x.IsMacro))
            BackgroundOperations.Add(command);
    }

    [RelayCommand]
    private async Task GetInnerMacros()
    {
        var existMacro = await existMacroService.GetInnerMacros();
        var existBackground = await existMacroService.GetInnerBackgroundOperations();

        foreach (var macro in existMacro)
        {
            var luafile = new LuaFile()
            {
                Name = macro.Name,
                Path = macro.Path,
                Content = macro.Content,
                IsSaved = true
            };
            
            InnerMacros.Add(luafile);
        }

        foreach (var backgroundOp in existBackground)
        {
            var luafile = new LuaFile()
            {
                Name = backgroundOp.Name,
                Path = backgroundOp.Path,
                Content = backgroundOp.Content,
                IsSaved = true
            };
            
            InnerBO.Add(luafile);
        }
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

        if (storageFile is null)
            return;

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

    [RelayCommand]
    private async Task AddToFavorite()
    {
        if (SelectedTab is null) return;

        if (SelectedTab.IsSaved)
        {
            await filesService.AddToFavorite(SelectedTab.Path);
            return;
        }

        var saveResult = await SaveRequested?.Invoke(SelectedTab, false)!;

        if (saveResult)
        {
            await filesService.AddToFavorite(SelectedTab.Path);
        }
    }

    void IRecipient<SelectFavoriteFileMessage>.Receive(SelectFavoriteFileMessage message)
    {
        using var file = File.OpenRead(message.Value);
        var text = File.ReadAllText(message.Value);

        var luaFile = new LuaFile()
        {
            Name = file.Name.Split('\\').Last(),
            Content = text,
            IsSaved = true,
            Path = message.Value,
        };

        Tabs.Add(luaFile);
    }

    public void Receive(SelectRecentFileMessage message)
    {
        using var file = File.OpenRead(message.Value);
        var text = File.ReadAllText(message.Value);

        var luaFile = new LuaFile()
        {
            Name = file.Name.Split('\\').Last(),
            Content = text,
            IsSaved = true,
            Path = message.Value,
        };

        Tabs.Add(luaFile);
    }

    [RelayCommand]
    private void CloseMacroPanel()
    {
        IsMacrosPanelVisible = false;
    }

    [RelayCommand]
    private void ShowTestMacro(string macroName)
    {
        var content = Properties.Resources.ResourceManager.GetString(macroName);

        if (content is null)
        {
            // TODO: show error
            return;
        }
        
        var testMacro = new LuaFile()
        {
            Name = macroName,
            Content = content.Replace("\\n", "\n").Replace("\\r", "\r"),
            IsSaved = true,
        };

        Tabs.Add(testMacro);
        SelectedTab = Tabs.Last();
    }
}
