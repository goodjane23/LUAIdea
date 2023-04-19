using LUAIdea.ViewModels;
using System.Windows;

namespace LUAIdea.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        DataContext = new MainWindowViewModel();
        
        InitializeComponent();
    }
}
