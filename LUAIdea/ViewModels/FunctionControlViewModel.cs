using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LUAIdea.Models;
using LUAIdea.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LUAIdea.ViewModels;
public class FunctionControlViewModel
{
    public FunctionModel CurrentModel { get; set; }
    public List<FunctionNodeModel> MacroNodes { get; private set; }

    public List<FunctionNodeModel> BacgroundOpNodes { get; private set; }
    IRelayCommand CloseCommand { get; set; }

    IRelayCommand HideCommand { get; set; }

    IRelayCommand DoubleClick { get; set; }

    public FunctionControlViewModel()
    {
        ApiCommandServices apiCommandServices = new ApiCommandServices();
        MacroNodes = apiCommandServices.GetMacroFunctionNode();
        BacgroundOpNodes = apiCommandServices.GetBackgoundOpFunctionNode();
        ApiCommandServices.GetAlldescriptionFromHttp();
    }
}
