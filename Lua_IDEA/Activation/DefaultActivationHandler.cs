using Lua_IDEA.Contracts.Services;
using Lua_IDEA.ViewModels;
using Microsoft.UI.Xaml;

namespace Lua_IDEA.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService navigationService;

    public DefaultActivationHandler(INavigationService navigationService)
    {
        this.navigationService = navigationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        return navigationService.Frame?.Content == null;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        navigationService.NavigateTo(typeof(MainPageViewModel).FullName!, args.Arguments);

        await Task.CompletedTask;
    }
}
