using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LUAIdea.Models;

public class MacroFileModel
{
    public string? Name { get; set; }

    public string? Path { get; set; }

    public string? Content { get; set; }
    public MacroFileModel()
    {
        
    }
    public MacroFileModel(string name, string path, string text)
    {
        Name = name;
        Path = path;
        Content = text;
    }
}