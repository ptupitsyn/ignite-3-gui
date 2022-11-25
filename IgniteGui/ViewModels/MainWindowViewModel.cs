
using ReactiveUI;

namespace IgniteGui.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string _connectionString = "localhost:10800";

    public string Greeting => "Welcome to Avalonia!";

    public string ConnectionString
    {
        get => _connectionString;
        set => this.RaiseAndSetIfChanged(ref _connectionString, value);
    }
}
