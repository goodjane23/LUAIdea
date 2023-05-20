using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Lua_IDEA.Messages;

public class SelectRecentFileMessage : ValueChangedMessage<string>
{
    public SelectRecentFileMessage(string value)
        : base(value)
    {
    }
}
