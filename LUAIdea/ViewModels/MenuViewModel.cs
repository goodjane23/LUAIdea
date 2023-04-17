using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LUAIdea.Models;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using System.Security.Cryptography;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using LUAIdea.Services;
using System.Threading.Tasks;
using System.Data;

namespace LUAIdea.ViewModels;

public partial class MenuViewModel : ObservableObject
{
    private FlowDocument flowDocument;
    private readonly ApiCommandServices apiCommandServices;

    [ObservableProperty]
    private string textContent;

    [ObservableProperty]
    private MacroFileModel selectedFile;

    [ObservableProperty] 
    private FunctionNodeModel selectedMacroModel;

    [ObservableProperty]
    private bool isMacroViewOpen = false;

    public ObservableCollection<FunctionNodeModel> MacroNodes { get; private set; }
 
    public ObservableCollection<FunctionNodeModel> BacgroundOpNodes { get; private set; }

    public ObservableCollection<MacroFileModel> Files { get; private set; }   

    public IRelayCommand NewFileCommand { get; }
    public IRelayCommand OpenFileCommand { get; }
    public IRelayCommand SaveCommand { get; }
    public IRelayCommand SaveAsCommand { get; }
    public IRelayCommand SaveAllCommand { get; }
    public IRelayCommand CloseCommand { get; }
    public IRelayCommand ShowMacroHelpCommand { get; }
    public IRelayCommand GetMacroDescriptionCommand { get; }
    public IRelayCommand GetBackgroundDescriptionCommand { get; }
    public IRelayCommand CloseMacroHelpCommand { get; set; }
    public IRelayCommand HideCommand { get; set; }
    public IRelayCommand DoubleClick { get; set; }
    public IRelayCommand UpadateFunctionsCommand { get; set; }

    public MenuViewModel()
    {
        NewFileCommand = new RelayCommand(CreateNewFile);
        OpenFileCommand = new RelayCommand(OpenFile);
        SaveCommand = new RelayCommand(SaveFile);
        SaveAsCommand = new RelayCommand(SaveAs);
        SaveAllCommand = new RelayCommand(SaveAll);
        CloseCommand = new RelayCommand(Close);
        ShowMacroHelpCommand = new RelayCommand(ShowMacroHelp);

        apiCommandServices = new ApiCommandServices();

        CloseMacroHelpCommand = new RelayCommand(CloseMacroHelp);
        HideCommand = new RelayCommand(Hide);
        UpadateFunctionsCommand = new AsyncRelayCommand(UpadateFunctions);      

        MacroNodes = new ObservableCollection<FunctionNodeModel>();
        BacgroundOpNodes = new ObservableCollection<FunctionNodeModel>();
        Files = new ObservableCollection<MacroFileModel>();
        flowDocument = new FlowDocument();
    }

    private void ShowMacroHelp()
    {
        IsMacroViewOpen = true;
    }   
    private void OpenFile()
    {
        try
        {           
            var dialog = new OpenFileDialog();
            dialog.DefaultExt = ".pm"; // Default file extension
            dialog.Filter = "Файлы макросов (.pm)|*.pm"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                var separate = dialog.FileName.Split('\\');
                var name = separate.Last();
                
                var path = dialog.FileName;                
               
                var text = File.ReadAllText(path);
                var macro = new MacroFileModel(name, path, text);

                Files.Add(macro);
                selectedFile = Files.FirstOrDefault();

                TextRange doc = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                using (FileStream fs = new FileStream(dialog.FileName, FileMode.Open))
                {
                    doc.Load(fs, DataFormats.Text);                   
                }

                textContent = doc.Text;

            }
        }
        catch (Exception)
        {

            throw;
        }
       
    }
    private void CreateNewFile()
    {
        try
        {
            var path = "";
            var name = Application.Current.TryFindResource("NewFileHeader").ToString();           
            var content = "";
            
            var macroModel = new MacroFileModel(name,path,content);
            Files.Add(macroModel);
            selectedFile = macroModel;

        }
        catch (Exception)
        {

            throw;
        }
        
    }
    private void SaveFile()
    {       
        if (string.IsNullOrWhiteSpace(selectedFile.Path))
        {
            SaveAs();
            return;
        }
        File.WriteAllText(selectedFile.Path, selectedFile.Content);       
    }
    private void SaveAs()
    {
        // Configure save file dialog box
        var dialog = new SaveFileDialog();
        dialog.FileName = "M"; // Default file name
        dialog.DefaultExt = ".pm"; // Default file extension
        dialog.Filter = "Файлы макросов (.pm)|*.pm"; // Filter files by extension

        // Show save file dialog box
        bool? result = dialog.ShowDialog();

        // Process save file dialog box results
        if (result == true)
        {
            // Save document
            string filename = dialog.FileName;
            File.WriteAllText(filename, selectedFile.Content);
        }
       
    }
    private void SaveAll() 
    {
        foreach (var file in Files)
        {
            selectedFile = file;
            SaveFile();
        }
    }
    private void Close()
    {
        Files.Remove(selectedFile);
        if (Files.Count>0)
        {
            selectedFile = Files.FirstOrDefault();
        }
    }

    private async Task UpadateFunctions()
    {
        var collection = await apiCommandServices.GetFunctionNode(5);

        foreach (FunctionNodeModel node in collection)
        {
            MacroNodes.Add(node);
        }
    }

    private void Hide()
    {
        IsMacroViewOpen = false;
    }

    private void CloseMacroHelp()
    {
        IsMacroViewOpen = false;
    }
}
