using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lua_IDEA.ViewModels;
public class ExistMacroViewModel
{
    private readonly ExistMacroViewModel viewModel;

    public ObservableCollection<string> ExistMacroList { get; set; } = new ObservableCollection<string>();
   
    public ExistMacroViewModel(ExistMacroViewModel viewModel)
    {

        this.viewModel = viewModel;
        GetExistList();
    }
    
    private void GetExistList()
    {
        foreach (var item in viewModel.ExistMacroList)
        {
            ExistMacroList.Add(item);
        }
    }

}
