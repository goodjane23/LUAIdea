using System.Collections.Generic;

namespace LUAIdea.Models;

public class FunctionNodeModel
{    
    public string? Header { get; set; }

    public ICollection<CommandModel> Functions { get; set; }
}
