using Lua_IDEA.Data;
using Lua_IDEA.Services;
using Lua_IDEA.ViewModels;
using Lua_IDEA.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using System.Net;

namespace Lua_IDEA;

public partial class App : Application
{
    public static Window MainWindow { get; private set; }

    private readonly IHost appHost;

    public App()
    {
        InitializeComponent();

        appHost = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                // Databse
                services.AddDbContextFactory<AppDbContext>(options =>
                {
                    options.UseSqlite("Data Source=database.db");
                });

                // Services
                services.AddSingleton<CommandService>();
                services.AddSingleton<NetworkChecker>();
                services.AddSingleton<SyntaxChecker>();

                // Windows
                services.AddSingleton<MainWindow>();

                // ViewModels
                services.AddSingleton<MainWindowViewModel>();
            })
            .Build();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var window = appHost.Services.GetRequiredService<MainWindow>();

        MainWindow = window;

        window.Activate();
    }    
}
