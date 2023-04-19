using CommunityToolkit.Mvvm.ComponentModel;

namespace LUAIdea.ViewModels;

public partial class EditFilePageViewModel : ObservableObject
{
    [ObservableProperty]
    private string fileContent;
}