using Lua_IDEA.Views;
using Microsoft.UI.Xaml;
using System.Net;

namespace Lua_IDEA;

public partial class App : Application
{
    public static Window MainWindow { get; private set; }
    
    private Window m_window;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();
        MainWindow = m_window;
        m_window.Activate();
    }    
}
