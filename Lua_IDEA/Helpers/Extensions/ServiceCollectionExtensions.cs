using Microsoft.Extensions.DependencyInjection;
using Lua_IDEA.Factories;
using Microsoft.UI.Xaml.Controls;

namespace Lua_IDEA.Helpers.Extensions;

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
