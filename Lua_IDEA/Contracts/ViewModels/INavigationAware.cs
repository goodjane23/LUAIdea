namespace Lua_IDEA.Contracts.ViewModels;

public interface INavigationAware
{
    void OnNavigatedTo(object parameter);
    void OnNavigatedFrom();
}
