using Lua_IDEA.Views;
using Microsoft.UI.Xaml;

namespace Lua_IDEA;

public partial class App : Application
{
    public static Window Current { get; private set; }

    private Window m_window;

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();
        Current = m_window;
        m_window.Activate();
    }
}
