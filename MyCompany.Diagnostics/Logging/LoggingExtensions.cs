using System;
using Serilog;
using Serilog.Events;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Serilog.Context;

namespace MyCompany.Diagnostics.Logging.Extension;

/// <summary>
/// Extensions for logging functionality
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Logs a message with the current activity context automatically included
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="level">The log level</param>
    /// <param name="message">The message template</param>
    /// <param name="propertyValues">The property values</param>
    public static void LogWithContext(this ILogger logger, LogEventLevel level, string message, params object[] propertyValues)
    {
        var activity = Activity.Current;
        
        if (activity != null)
        {
            using (LogContext.PushProperty("TraceId", activity.TraceId.ToString()))
            using (LogContext.PushProperty("SpanId", activity.SpanId.ToString()))
            {
                if (activity.ParentSpanId != default)
                {
                    using (LogContext.PushProperty("ParentSpanId", activity.ParentSpanId.ToString()))
                    {
                        logger.Write(level, message, propertyValues);
                    }
                }
                else
                {
                    logger.Write(level, message, propertyValues);
                }
            }
        }
        else
        {
            logger.Write(level, message, propertyValues);
        }
    }

    /// <summary>
    /// Adds tag properties from a dictionary to the log context
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="tags">Dictionary of tags to add</param>
    /// <returns>A disposable that will remove the tags when disposed</returns>
    public static IDisposable WithTags(this ILogger logger, Dictionary<string, string> tags)
    {
        var disposables = new List<IDisposable>();
        
        foreach (var tag in tags)
        {
            disposables.Add(LogContext.PushProperty(tag.Key, tag.Value));
        }
        
        return new CompositeDisposable(disposables);
    }

    /// <summary>
    /// A disposable that disposes multiple disposables when disposed
    /// </summary>
    private class CompositeDisposable : IDisposable
    {
        private readonly List<IDisposable> _disposables;

        public CompositeDisposable(List<IDisposable> disposables)
        {
            _disposables = disposables;
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables.Reverse<IDisposable>())
            {
                disposable.Dispose();
            }
        }
    }
}