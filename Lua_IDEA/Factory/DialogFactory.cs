using System;

namespace Lua_IDEA.Factory;

public class DialogFactory<TDialog>
{
    private readonly Func<TDialog> factory;

    public DialogFactory(Func<TDialog> factory)
    {
        this.factory = factory;
    }

    public TDialog Create()
    {
        return factory.Invoke();
    }
}