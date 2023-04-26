﻿using System.Collections.Generic;

namespace Lua_IDEA.Entities;

public class CommandCategory
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Command> Commands { get; set; }

   
}
