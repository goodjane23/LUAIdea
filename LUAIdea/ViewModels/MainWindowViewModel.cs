using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using LUAIdea.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using LUAIdea.Services;
using System.Threading.Tasks;
using System.Windows.Controls;
using LUAIdea.Views;

namespace LUAIdea.ViewModels;

public partial class MainWindowViewModel : ObservableObject
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
    private bool isMacroViewOpen;

    [ObservableProperty]
    private CommandModel selectedCommand;

    public ObservableCollection<FunctionNodeModel> MacroNodes { get; private set; }
    public ObservableCollection<FunctionNodeModel> BackgroundOpNodes { get; private set; }
    public ObservableCollection<MacroFileModel> Files { get; private set; }

    #region Commands
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
    public IRelayCommand UpdateFunctionsCommand { get; set; }
    public IRelayCommand<CommandModel> DoubleClickTVCommand { get; set; }

    #endregion

    public MainWindowViewModel()
    {
        NewFileCommand = new RelayCommand(CreateNewFile);
        OpenFileCommand = new RelayCommand(OpenFile);
        SaveCommand = new RelayCommand(SaveFile);
        SaveAsCommand = new RelayCommand(SaveAs);
        SaveAllCommand = new RelayCommand(SaveAll);
        CloseCommand = new RelayCommand(Close);
        ShowMacroHelpCommand = new RelayCommand(ShowMacroHelp);
        DoubleClickTVCommand = new RelayCommand<CommandModel>(DoubleClickTV);
        apiCommandServices = new ApiCommandServices();

        CloseMacroHelpCommand = new RelayCommand(CloseMacroHelp);
        HideCommand = new RelayCommand(Hide);
        UpdateFunctionsCommand = new AsyncRelayCommand(UpdateFunctions);      

        MacroNodes = new ObservableCollection<FunctionNodeModel>();
        BackgroundOpNodes = new ObservableCollection<FunctionNodeModel>();
        Files = new ObservableCollection<MacroFileModel>();
        flowDocument = new FlowDocument();
    }

    private void DoubleClickTV(CommandModel command)
    {
        SelectedFile.Content += command?.Name;
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedFile)));
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
            
            dialog.DefaultExt = ".pm";
            dialog.Filter = "Файлы макросов (.pm)|*.pm";

            // Show open file dialog box
            var result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == false)
                return;
            
            var separate = dialog.FileName.Split('\\');
            var name = separate.Last();
                
            var path = dialog.FileName;                
               
            var text = File.ReadAllText(path);
            var macro = new MacroFileModel()
            {
                Name = name,
                Path = path,
                Content = text
            };

            Files.Add(macro);
            SelectedFile = Files.FirstOrDefault();

            var doc = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                
            using (var fs = new FileStream(dialog.FileName, FileMode.Open))
            {
                doc.Load(fs, DataFormats.Text);                   
            }

            TextContent = doc.Text;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
    
    private void CreateNewFile()
    {
        try
        {
            var path = "";
            var name = Application.Current.TryFindResource("NewFileHeader").ToString();           
            var content = "";
            
            var macroModel = new MacroFileModel()
            {
                Name = name,
                Path = path,
                Content = content,
                OpenedPage = new EditFilePage()
            };
            
            Files.Add(macroModel);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
    
    private void SaveFile()
    {       
        if (string.IsNullOrWhiteSpace(SelectedFile.Path))
        {
            SaveAs();
            return;
        }
        
        File.WriteAllText(SelectedFile.Path, SelectedFile.Content);       
    }
    
    private void SaveAs()
    {
        // Configure save file dialog box
        var dialog = new SaveFileDialog();
        
        dialog.FileName = "M";
        dialog.DefaultExt = ".pm";
        dialog.Filter = "Файлы макросов (.pm)|*.pm";

        // Show save file dialog box
        var result = dialog.ShowDialog();

        // Process save file dialog box results
        if (result == false)
            return;
        
        var filename = dialog.FileName;
        File.WriteAllText(filename, SelectedFile.Content);

    }
    
    private void SaveAll() 
    {
        foreach (var file in Files)
        {
            SelectedFile = file;
            SaveFile();
        }
    }
    
    private void Close()
    {
        Files.Remove(selectedFile);
        
        if (Files.Count > 0)
        {
            SelectedFile = Files.FirstOrDefault();
        }
    }

    private async Task UpdateFunctions()
    {
        var collection = await apiCommandServices.GetFunctionNode(5);

        foreach (var node in collection)
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
