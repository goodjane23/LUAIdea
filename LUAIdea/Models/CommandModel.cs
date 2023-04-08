using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LUAIdea.Models
{
    class FunctionModel : IFunctionModel
    {
        public string? Name { get; set; }

        public string? Desription { get; set; }

        public string? Function { get; set; }

        public bool IsMacroFunction { get; set; }
    }
}
