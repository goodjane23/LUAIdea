using CommunityToolkit.Mvvm.ComponentModel;
using Lua_IDEA.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lua_IDEA.ViewModels;
public partial class RecentFilesDialogSelectorViewModel : ObservableObject
{
    private readonly FilesServise filesServise;

    ObservableCollection<string> RecentPath { get; set; } = new();

    [ObservableProperty]
    private string selectedPath;

    public RecentFilesDialogSelectorViewModel(FilesServise favoriteService)
    {
        this.filesServise = favoriteService;
    }
}
