using Lua_IDEA.Contracts.Services;
using Lua_IDEA.Data.Entities;
using Lua_IDEA.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Lua_IDEA.Views.Dialogs;

public sealed partial class FavoriteFileSelector
{
    private FavoriteFile? currentFile;
   
    private readonly FavoriteFileSelectorViewModel viewModel;

    public FavoriteFileSelector(FavoriteFileSelectorViewModel viewModel)
    {
        InitializeComponent();

        this.viewModel = viewModel;
    }

    
    private void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        viewModel.GetFavoritesFilesCommand.Execute(null);
    }

    private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        viewModel.RemoveFileCommand.Execute(currentFile?.Path);
    }

    private void MenuFlyout_Opening(object sender, object e)
    {
        var item = sender as MenuFlyout;
        currentFile = (item?.Target as ListViewItem).Content as FavoriteFile;
    }
}
