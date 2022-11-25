
using ReactiveUI;

namespace IgniteGui.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string _connectionString = "localhost:10800";
    private string _status = "Not connected.";

    public string ConnectionString
    {
        get => _connectionString;
        set => this.RaiseAndSetIfChanged(ref _connectionString, value);
    }

    public string Status
    {
        get => _status;
        private set => this.RaiseAndSetIfChanged(ref _status, value);
    }
}
