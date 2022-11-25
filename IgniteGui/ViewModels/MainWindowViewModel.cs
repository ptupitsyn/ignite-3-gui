﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Apache.Ignite;
using Apache.Ignite.Table;
using ReactiveUI;

namespace IgniteGui.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);

    private string _connectionString = "localhost:10942";
    private string _status = "Not connected.";
    private bool _isLoading;
    private IIgniteClient? _client;
    private IReadOnlyList<ITable> _tables = Array.Empty<ITable>();

    public MainWindowViewModel()
    {
        ConnectCommand = ReactiveCommand.CreateFromTask(Connect);

        _ = Connect();
    }

    public string ConnectionString {
        get => _connectionString;
        set => this.RaiseAndSetIfChanged(ref _connectionString, value);
    }

    public string Status { get => _status; private set => this.RaiseAndSetIfChanged(ref _status, value); }

    public bool IsLoading { get => _isLoading; private set => this.RaiseAndSetIfChanged(ref _isLoading, value); }

    public bool IsConnected => _client != null;

    public IReadOnlyList<ITable> Tables { get => _tables; private set => this.RaiseAndSetIfChanged(ref _tables, value); }

    public ICommand ConnectCommand { get; }

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
            this.RaisePropertyChanged(nameof(IsConnected));
        }
    }
}
