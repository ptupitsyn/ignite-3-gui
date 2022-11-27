
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Apache.Ignite;
using Apache.Ignite.Network;
using Apache.Ignite.Sql;
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

    [ObservableProperty] private IList<ITable> _tables = Array.Empty<ITable>();

    [ObservableProperty] private IList<IClusterNode> _nodes = Array.Empty<IClusterNode>();

    [ObservableProperty] private ITable? _selectedTable;

    [ObservableProperty] private string _query = string.Empty;

    [ObservableProperty] private string _queryResult = string.Empty;

    private IIgniteClient? _client;

    public MainWindowViewModel()
    {
        Task.Run(Connect);
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
                Tables = await _client.Tables.GetTablesAsync();
                Nodes = await _client.GetClusterNodesAsync();
                Status = "Connected";
            }
            else
            {
                _client?.Dispose();
                _client = null;
                Tables = Array.Empty<ITable>();
                Nodes = Array.Empty<IClusterNode>();
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

    [RelayCommand]
    private async Task RunQuery()
    {
        if (_client == null)
        {
            QueryResult = "Not connected.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Query))
        {
            QueryResult = "Query is empty.";
            return;
        }

        IsLoading = true;

        try
        {
            await using var resultSet = await _client.Sql.ExecuteAsync(null, _query);
            await ReadQueryResult(resultSet);
        }
        catch (Exception e)
        {
            QueryResult = "Failed to execute query: " + e;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ReadQueryResult(IResultSet<IIgniteTuple> resultSet)
    {
        if (!resultSet.HasRowSet)
        {
            QueryResult = $"No row set. Applied: {resultSet.WasApplied}. Affected rows: {resultSet.AffectedRows}.";
            return;
        }

        var sb = new StringBuilder();

        // Header.
        foreach (var column in resultSet.Metadata!.Columns)
        {
            sb.Append(column.Name)
                .Append(" (")
                .Append(column.Type)
                .Append("), ");
        }

        sb.AppendLine();

        // Rows.
        await foreach (var row in resultSet)
        {
            for (int i = 0; i < row.FieldCount; i++)
            {
                sb.Append(row[i] ?? "NULL").Append(", ");
            }

            sb.AppendLine();
        }

        QueryResult = sb.ToString();
    }

    partial void OnSelectedTableChanged(ITable? value)
    {
        if (value != null && string.IsNullOrWhiteSpace(Query))
        {
            Query = $"select * from {value.Name} limit 10";
        }
    }
}
