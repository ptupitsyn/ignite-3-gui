using Avalonia.Controls;
using Avalonia.Interactivity;
using IgniteGui.ViewModels;

namespace IgniteGui.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ListBox_OnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        ((MainWindowViewModel?)DataContext)?.GenerateQuery();
    }
}
