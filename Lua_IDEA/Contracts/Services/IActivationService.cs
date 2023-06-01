namespace Lua_IDEA.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
