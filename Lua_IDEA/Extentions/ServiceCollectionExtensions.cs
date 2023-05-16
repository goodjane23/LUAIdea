using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using Lua_IDEA.Factory;
using Microsoft.UI.Xaml.Controls;

namespace Lua_IDEA.Extentions;
public static class ServiceCollectionExtensions
{
    public static void AddWindowFactory<TContentDialog>(this IServiceCollection services)
       where TContentDialog : ContentDialog
    {
        services.AddTransient<TContentDialog>();
        services.AddSingleton<Func<TContentDialog>>(x => () => x.GetRequiredService<TContentDialog>());
        services.AddSingleton<WindowFactory<TContentDialog>>();
    }
}
