
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

    [ObservableProperty] private IList<ClusterNode> _nodes = Array.Empty<ClusterNode>();

    [ObservableProperty] private IList<IClusterNode> _connections = Array.Empty<IClusterNode>();

    [ObservableProperty] private ITable? _selectedTable;

    [ObservableProperty] private string _query = string.Empty;

    [ObservableProperty] private string _queryResult = string.Empty;

    private IIgniteClient? _client;

    public MainWindowViewModel()
    {
        Task.Run(Init);
    }

    public bool IsConnected => _client != null;

    public ObservableCollection<ObservableCollectionLogger.Entry> Log { get; } = new();

    public void GenerateQuery()
    {
        if (SelectedTable != null)
        {
            Query = $"select * from {SelectedTable.Name} limit 10";
        }
    }

    private async Task Init()
    {
        // Small delay to let the UI update.
        await Task.Delay(1000);

        // Init.
        await Connect();
        await Refresh();
    }

    [RelayCommand]
    private async Task Connect()
    {
        IsLoading = true;
        await _connectionSemaphore.WaitAsync();

        try
        {
            if (_client == null)
            {
                var cfg = new IgniteClientConfiguration(ConnectionString)
                {
                    Logger = new ObservableCollectionLogger(Log)
                };

                _client = await IgniteClient.StartAsync(cfg);
                Tables = await _client.Tables.GetTablesAsync();
                Nodes = (await _client.GetClusterNodesAsync())
                    .Select(n => new ClusterNode(n.Id, n.Name, n.Address.ToString()))
                    .ToList();

                Status = "Connected";
            }
            else
            {
                _client?.Dispose();
                _client = null;
                Tables = Array.Empty<ITable>();
                Nodes = Array.Empty<ClusterNode>();

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

    private async Task Refresh()
    {
        while (true)
        {
            if (_client != null)
            {
                try
                {
                    var connections = _client.GetConnections();

                    if (connections.Count != Connections.Count ||
                        connections.Select(c => c.Id).Except(Connections.Select(c => c.Id)).Any())
                    {
                        Connections = connections;
                    }
                    {
                        Connections = connections;
                    }
                }
                catch (Exception e)
                {
                    Status = "Failed to refresh: " + e.Message;
                }
            }

            await Task.Delay(15_000);
        }
    }

    partial void OnSelectedTableChanged(ITable? value)
    {
        if (string.IsNullOrWhiteSpace(Query))
        {
            GenerateQuery();
        }
    }

    public record ClusterNode(string Id, string Name, string Address);
}
