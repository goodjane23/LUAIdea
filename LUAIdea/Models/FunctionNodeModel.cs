using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LUAIdea.Models;
public class FunctionNodeModel
{
    public string Header { get; set; }

    public List<IFunctionModel> Functions { get; set; }
    public FunctionNodeModel()
    {
        Functions = new List<IFunctionModel>();        
    }
}
