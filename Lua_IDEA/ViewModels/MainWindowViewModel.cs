using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lua_IDEA.Data.Entities;
using Lua_IDEA.Models;
using Lua_IDEA.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using CommunityToolkit.Mvvm.Messaging;
using Lua_IDEA.Messages;
using System.IO;

namespace Lua_IDEA.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IRecipient<SelectRecentFileMessage>
{
    public event Action CommandPasted;
    public event Func<LuaFile, Task> SaveRequested;
    public event Func<Task<bool>> SaveCheckRequested;
    public event Func<LuaFile, Task<bool>> CloseRequested;

    private readonly CommandService commandService;
    private readonly SyntaxChecker syntaxChecker;
    private readonly FilesServise filesService;

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
        FilesServise filesService)
    {
        this.commandService = commandService;
        this.syntaxChecker = syntaxChecker;
        this.filesService = filesService;
        WeakReferenceMessenger.Default.Register<SelectRecentFileMessage>(this);
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

    public async void Receive(SelectRecentFileMessage message)
    {
        var text = await File.ReadAllTextAsync(message.Value);
        LuaFile luaFile = new LuaFile()
        {
            Name = "jdhksjh",
            Content = text,
            IsSaved = true,
            Path = message.Value,
        };
        Tabs.Add(luaFile);
    }

    [RelayCommand]
    private async void ShowTestMacro()
    {
        var testMacro = new LuaFile()
        {
            Name = "Test Macro",
            Content = "function M_XXX() \r\n\r\n local outputs_count = 8\r\n " +
            "for i = 0, outputs_count  do \r\n" +
            "if (PinGetState(Outputs.UserOutput_0 + i)) then\r\n DisplayMessage(\"Output_\"..i..\" ON\")\r\n else\r\n DisplayMessage(\"Output_\"..i..\" OFF\")\r\n end\r\n end end",
            IsFavorite = false,
            IsSaved = false,
        };
        Tabs.Add(testMacro);
        SelectedTab = Tabs.Last();

    }

    [RelayCommand]
    private async void ShowHeightMap() 
    {
        var testMacro = new LuaFile()
        {
            Name = "M155",
            Content = "function m155()\r\n    local XWidth = 70\r\n    local YWidth = 50\r\n    local SafeZ  = 3\r\n    local ProbeZ = -3\r\n    local StepX  = 15\r\n    local StepY  = 15\r\n    local Feed   = 50\r\n    local TipHeight = 0\r\n    local ProbeFilename = \"C:\\\\temp\\\\probe.txt\"\r\n     \r\n    PushCurrentDistanceMode()\r\n    PushCurrentMotionMode()\r\n     \r\n    if (IsProbingPinConfigured()) then\r\n        -- open the file\r\n        file, msg = io.open(ProbeFilename, \"w\")\r\n         \r\n        if (file == nil) then\r\n            DisplayMessage(\"Could not open probe output file (\"..msg..\")\")\r\n            Stop()\r\n            return\r\n        end\r\n         \r\n        ExecuteMDI(\"F \"..Feed)\r\n        ExecuteMDI(\"G90 G38.2 Z-100\")\r\n         \r\n        -- set the current location to 0,0,0\r\n        ExecuteMDI(\"G92 X0Y0Z0\")\r\n        ExecuteMDI(\"G0 Z\"..SafeZ)\r\n         \r\n        local direction = 0\r\n        for y = 0, YWidth, StepY do\r\n            if (direction == 1) then\r\n                direction = 0\r\n            else\r\n                direction = 1\r\n            end\r\n             \r\n            for x = 0, XWidth, StepX do\r\n                if (direction == 1) then\r\n                    ExecuteMDI(\"G0 X\"..x..\" Y\"..y..\" Z\"..SafeZ)\r\n                else\r\n                    ExecuteMDI(\"G0 X\"..(XWidth - x)..\" Y\"..y..\" Z\"..SafeZ)\r\n                end\r\n                 \r\n                ExecuteMDI(\"G38.2 Z\"..ProbeZ)\r\n                LogCurrentPos(TipHeight)\r\n                ExecuteMDI(\"G0 Z\"..SafeZ)\r\n            end\r\n        end\r\n         \r\n        if (direction == 1) then\r\n            ExecuteMDI(\"G0 X\"..XWidth..\" Y\"..YWidth..\" Z\"..SafeZ)\r\n        else\r\n            ExecuteMDI(\"G0 X\"..\"0\"..\" Y\"..YWidth..\" Z\"..SafeZ)\r\n        end\r\n         \r\n        local HighZ = 5\r\n        ExecuteMDI(\"G0 Z\"..HighZ)\r\n        ExecuteMDI(\"G0 X0Y0\")\r\n         \r\n        file:close()\r\n    else\r\n        DisplayMessage(\"Probe input is not configured\")\r\n        return\r\n    end\r\nend\r\n \r\nfunction LogCurrentPos(tipHeight)\r\n    local CurrX = AxisGetPos(Axis.X)\r\n    local CurrY = AxisGetPos(Axis.Y)\r\n    local CurrZ = AxisGetPos(Axis.Z)\r\n     \r\n    local fmt = \"%.5f\"\r\n    file:write(string.format(fmt, CurrX)..\",\"..string.format(fmt, CurrY)..\",\"..string.format(fmt, CurrZ - tipHeight), \"\\n\")\r\nend",
            IsFavorite = false,
            IsSaved = false,
        };
        Tabs.Add(testMacro);
        SelectedTab = Tabs.Last();
    }


    private void OpenSelectedFile(string path)
    {

    }


}
