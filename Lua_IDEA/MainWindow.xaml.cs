using Microsoft.UI.Xaml;

namespace Lua_IDEA;

public sealed partial class MainWindow : Window
{
    private int clicksCount;

    public MainWindow()
    {
        this.InitializeComponent();
    }

    private void myButton_Click(object sender, RoutedEventArgs e)
    {
        myButton.Content = $"Clicked {++clicksCount} times";
    }
}
