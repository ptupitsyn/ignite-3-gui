
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apache.Ignite;
using Apache.Ignite.Table;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace IgniteGui.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);

    [ObservableProperty] private string _connectionString = "localhost:10942";

    [ObservableProperty] private string _status = "Not connected.";

    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private IReadOnlyList<ITable> _tables = Array.Empty<ITable>();

    [ObservableProperty] private ITable? _selectedTable;

    [ObservableProperty] private string _query = string.Empty;

    private IIgniteClient? _client;

    public MainWindowViewModel()
    {
        _ = Connect();
    }

    public bool IsConnected => _client != null;

    [RelayCommand]
    private async Task Connect()
    {
        IsLoading = true;
        await _connectionSemaphore.WaitAsync();

        try
        {
            if (_client == null)
            {
                _client = await IgniteClient.StartAsync(new IgniteClientConfiguration(ConnectionString));
                Tables = (await _client.Tables.GetTablesAsync()).ToList();
                Status = "Connected";
            }
            else
            {
                _client?.Dispose();
                _client = null;
                Tables = Array.Empty<ITable>();
                Status = "Disconnected";
            }
        }
        catch (Exception e)
        {
            Status = "Failed to connect: " + e.Message;
        }
        finally
        {
            IsLoading = false;
            _connectionSemaphore.Release();
            OnPropertyChanged(nameof(IsConnected));
        }
    }

    partial void OnSelectedTableChanged(ITable? value)
    {
        if (value != null)
        {
            Query = "select * from " + value.Name;
        }
    }
}
