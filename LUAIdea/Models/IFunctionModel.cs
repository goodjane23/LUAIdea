using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LUAIdea.Models;
public interface IFunctionModel
{
    public string? Name { get; set; }

    public string? Desription { get; set; }

    public string? Function { get; set; }
}
