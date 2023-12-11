using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace IgniteGui.ViewModels;

public class ObservableCollectionLoggerFactory : ILoggerFactory
{
    private readonly ObservableCollection<ObservableCollectionLogger.Entry> _log;

    public ObservableCollectionLoggerFactory(ObservableCollection<ObservableCollectionLogger.Entry> log)
    {
        _log = log;
    }

    public void Dispose()
    {
        // No-op.
    }

    public ILogger CreateLogger(string categoryName) => new ObservableCollectionLogger(_log, categoryName);

    public void AddProvider(ILoggerProvider provider)
    {
        throw new System.NotImplementedException();
    }
}
