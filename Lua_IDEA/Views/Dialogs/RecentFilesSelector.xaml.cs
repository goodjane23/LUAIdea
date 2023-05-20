using Lua_IDEA.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Lua_IDEA.Views.Dialogs;

public sealed partial class RecentFilesSelector : ContentDialog
{
    private readonly RecentFilesSelectorViewModel viewModel;

    public RecentFilesSelector(RecentFilesSelectorViewModel viewModel)
    {
        this.viewModel = viewModel;

        this.InitializeComponent();      
    }

    private void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        viewModel.GetRecentPathsCommand.Execute(null);
    }
}
