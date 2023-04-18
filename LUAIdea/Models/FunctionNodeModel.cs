using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LUAIdea.Models;
public class FunctionNodeModel
{    
    public string? Header { get; set; }

    public ICollection<FunctionModel> Functions { get; set; }
   
}
