using Lua_IDEA.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Lua_IDEA.Views;

public sealed partial class MainWindow : Window
{
    private readonly MainWindowViewModel viewModel;

    public MainWindow()
    {
        this.InitializeComponent();

        viewModel = new MainWindowViewModel();

        //var window = WindowHelper.GetWindowForElement();
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

    }

    private void TabView_AddTabButtonClick(TabView sender, object args)
    {
        viewModel.AddFileCommand.Execute(null);
    }

    private void TreeViewItem_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        viewModel.PasteCommand.Execute(null);
    }

    private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        viewModel.CloseFileCommand.Execute(args.Item);
    }
}