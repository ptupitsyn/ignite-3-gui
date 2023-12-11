using System;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;

namespace IgniteGui.ViewModels;

public class ObservableCollectionLogger : ILogger
{
    private readonly ObservableCollection<Entry> _log;
    private readonly string _category;

    public ObservableCollectionLogger(ObservableCollection<Entry> log, string category)
    {
        _log = log;
        _category = category;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var msg = formatter(state, exception);

        Dispatcher.UIThread.Post(() => _log.Add(new Entry(DateTime.Now, logLevel, msg, _category)));
    }

    public bool IsEnabled(LogLevel level) => level >= LogLevel.Debug;
    public IDisposable BeginScope<TState>(TState state)
    {
        throw new NotImplementedException();
    }

    public record Entry(DateTime DateTime, LogLevel Level, string Message, string? Category);
}
