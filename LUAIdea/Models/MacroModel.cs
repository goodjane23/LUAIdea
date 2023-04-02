using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LUAIdea.Models;

public class MacroModel
{
    public string? Name { get; set; }

    public string? Path { get; set; }

    public string? Content { get; set; }
    public MacroModel(string name, string path, string text)
    {
        Name = name;
        Path = path;
        Content = text;
    }
}