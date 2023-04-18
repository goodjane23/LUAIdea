using LUAIdea.ViewModels;
using System.Windows;

namespace LUAIdea;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        DataContext = new MainWindowViewModel();
        
        InitializeComponent();
    }
}
