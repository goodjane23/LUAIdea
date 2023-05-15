using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lua_IDEA.ViewModels;
public partial class ListFilesControlViewModel : ObservableObject
{
    private string? resultPath;

    [ObservableProperty]
    private string? selectedPath;
    
    public ObservableCollection<string> Paths { get; set; }= new ObservableCollection<string>();

    public ListFilesControlViewModel(IEnumerable<string> values)
    {
        foreach (var value in values)
        {
            Paths.Add(value);
        }
    }

    [RelayCommand]
    private void OkButtonPress()
    {
        if (SelectedPath is not null)
            resultPath = SelectedPath;
    }

    [RelayCommand]
    private void CancelButtonPress()
    {
        resultPath = null;
    }

}
