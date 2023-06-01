using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Lua_IDEA.Messages;

class SelectFavoriteFileMessage : ValueChangedMessage<string>
{
    public SelectFavoriteFileMessage(string value)
        : base(value)
    {
    }
}
