using System;
using System.Collections.ObjectModel;
using Apache.Ignite.Log;

namespace IgniteGui.ViewModels;

public class ObservableCollectionLogger : IIgniteLogger
{
    private readonly ObservableCollection<Entry> _log;

    public ObservableCollectionLogger(ObservableCollection<Entry> log)
    {
        _log = log;
    }

    public void Log(LogLevel level, string message, object[]? args, IFormatProvider? formatProvider, string? category,
        string? nativeErrorInfo, Exception? ex)
    {
        var msg = args == null ? message : string.Format(formatProvider, message, args);

        if (ex != null)
        {
            msg += $"(error: '{ex.Message}')";
        }

        _log.Add(new Entry(DateTime.Now, level, msg, category));
    }

    public bool IsEnabled(LogLevel level) => level >= LogLevel.Debug;

    public record Entry(DateTime DateTime, LogLevel Level, string Message, string? Category);
}
