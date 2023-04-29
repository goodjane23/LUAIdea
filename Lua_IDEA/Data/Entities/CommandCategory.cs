using System.Collections.Generic;

namespace Lua_IDEA.Data.Entities;

public class CommandCategory
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsMacro { get; set; }

    public ICollection<Command> Commands { get; set; }
}
