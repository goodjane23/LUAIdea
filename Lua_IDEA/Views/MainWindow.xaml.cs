using Lua_IDEA.Entities;
using Lua_IDEA.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Lua_IDEA.Views;

public sealed partial class MainWindow : Window
{
    private readonly MainWindowViewModel viewModel;

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(titleBar);
        
        viewModel = new MainWindowViewModel();
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

    private void TextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        viewModel.TextChangingCommand.Execute(null);
    }
}