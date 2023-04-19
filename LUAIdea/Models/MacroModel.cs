using System;
using LUAIdea.Views;

namespace LUAIdea.Models;

public class MacroFileModel
{
    public Guid Guid { get; set; } = Guid.NewGuid();
    
    public string? Name { get; set; }

    public string? Path { get; set; }

    public string? Content { get; set; }
    
    public EditFilePage OpenedPage { get; set; }
}