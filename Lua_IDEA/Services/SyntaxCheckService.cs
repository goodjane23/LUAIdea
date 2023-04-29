using MoonSharp.Interpreter;
using System;

namespace Lua_IDEA.Services;

class SyntaxCheckerService
{
    public string CheckSyntax(string text)
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
