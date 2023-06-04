using Lua_IDEA.Activation;
using Lua_IDEA.Contracts.Services;
using Lua_IDEA.Core.Contracts.Services;
using Lua_IDEA.Core.Services;
using Lua_IDEA.Data;
using Lua_IDEA.Helpers.Extensions;
using Lua_IDEA.Models;
using Lua_IDEA.Services;
using Lua_IDEA.ViewModels;
using Lua_IDEA.Views.Dialogs;
using Lua_IDEA.Views.Pages;
using Lua_IDEA.Views.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using System.Diagnostics;

namespace Lua_IDEA;

public partial class App : Application
{
    public static WindowEx MainWindow { get; } = new MainWindow();
    public static UIElement? AppTitlebar { get; set; }

    private readonly IHost appHost;

    public App()
    {
        InitializeComponent();

        appHost = Host.CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices((context, services) =>
            {
                // Default Activation Handler
                services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

                // Database
                services.AddDbContextFactory<AppDbContext>(options =>
                {
                    options.UseSqlite("Data Source=database.db");
                });

                services.AddDialogFactory<RecentFilesSelector>();
                services.AddDialogFactory<FavoriteFileSelector>();

                // Services
                services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
                services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
                services.AddSingleton<IActivationService, ActivationService>();
                services.AddSingleton<IPageService, PageService>();
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IFileService, FileService>();
                services.AddSingleton<ParsingMacroAPIService>();
                services.AddSingleton<NetworkChecker>();
                services.AddSingleton<SyntaxChecker>();
                services.AddSingleton<FilesService>();

                // Views
                services.AddTransient<SettingsPage>();
                services.AddTransient<ShellPage>();
                services.AddTransient<MainPage>();

                // ViewModels
                services.AddSingleton<SettingsViewModel>();
                services.AddSingleton<ShellViewModel>();
                services.AddSingleton<MainPageViewModel>();
                services.AddSingleton<RecentFilesSelectorViewModel>();
                services.AddSingleton<FavoriteFileSelectorViewModel>();

                // Configuration
                services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
            })
            .Build();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Debug.WriteLine(e.Exception.Message);
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        await GetService<IActivationService>().ActivateAsync(args);
    }

    public static T GetService<T>()
        where T : class
    {
        var service = (App.Current as App)!.appHost.Services.GetRequiredService<T>();

        return service;
    }
}
