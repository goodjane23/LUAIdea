using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Lua_IDEA.Factory;
public class WindowFactory<TContentDialog>
{
    private readonly Func<TContentDialog> factory;

    public WindowFactory(Func<TContentDialog> factory)
    {
        this.factory = factory;
    }

    public TContentDialog Create()
    {
        return factory.Invoke();
    }
}