using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lua_IDEA.Services;

class SyntaxCheckService
{
    public static string SyntaxCheck(string text)
    {
        Script script = new Script();
        string result;
		try
		{
            var res = script.DoString(text);
            
        }
		catch (Exception ex)
		{
            return result = ex.Message;
		}

        return "";
    }
}
