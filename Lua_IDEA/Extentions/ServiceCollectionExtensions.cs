using Microsoft.Extensions.DependencyInjection;
using System;
using Lua_IDEA.Factory;
using Microsoft.UI.Xaml.Controls;

namespace Lua_IDEA.Extentions;

public static class ServiceCollectionExtensions
{
    public static void AddDialogFactory<TDialog>(this IServiceCollection services)
       where TDialog : ContentDialog
    {
        services.AddTransient<TDialog>();
        services.AddSingleton<Func<TDialog>>(x => () => x.GetRequiredService<TDialog>());
        services.AddSingleton<DialogFactory<TDialog>>();
    }
}
