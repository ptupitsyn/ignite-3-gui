using System;
using System.Collections.ObjectModel;
using Apache.Ignite.Log;

namespace IgniteGui.ViewModels;

public class ObservableCollectionLogger : IIgniteLogger
{
    private readonly ObservableCollection<string> _log;

    public ObservableCollectionLogger(ObservableCollection<string> log)
    {
        _log = log;
    }

    public void Log(LogLevel level, string message, object[]? args, IFormatProvider? formatProvider, string? category,
        string? nativeErrorInfo, Exception? ex)
    {
        var msg = args == null ? message : string.Format(formatProvider, message, args);

        msg = $"[{level}] {msg}";

        if (ex != null)
        {
            msg += $"(error: '{ex.Message}')";
        }

        _log.Add(msg);
    }

    public bool IsEnabled(LogLevel level) => level >= LogLevel.Debug;
}
