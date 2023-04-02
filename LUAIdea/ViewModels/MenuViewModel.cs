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

namespace LUAIdea.ViewModels;

public partial class MenuViewModel : ObservableObject
{
    private FlowDocument flowDocument;

    [ObservableProperty]
    private string textContent;

    [ObservableProperty]
    private MacroModel selectedFile;

    public ObservableCollection<MacroModel> Files { get; private set; }    

    public IRelayCommand NewFileCommand { get; }
    public IRelayCommand OpenFileCommand { get; }
    public IRelayCommand SaveCommand { get; }
    public IRelayCommand SaveAsCommand { get; }
    public IRelayCommand SaveAllCommand { get; }
    public IRelayCommand CloseCommand { get; }

    public MenuViewModel()
    {
        NewFileCommand = new RelayCommand(CreateNewFile);
        OpenFileCommand = new RelayCommand(OpenFile);
        SaveCommand = new RelayCommand(SaveFile);
        SaveAsCommand = new RelayCommand(SaveAs);
        SaveAllCommand = new RelayCommand(SaveAll);
        CloseCommand = new RelayCommand(Close);

        Files = new ObservableCollection<MacroModel>();
        flowDocument = new FlowDocument();
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
                var macro = new MacroModel(name, path, text);

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
            
            var macroModel = new MacroModel(name,path,content);
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
}
