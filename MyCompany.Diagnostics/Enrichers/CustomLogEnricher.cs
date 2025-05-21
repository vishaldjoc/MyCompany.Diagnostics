using Serilog.Core;
using Serilog.Events;
using MyCompany.Diagnostics.Configuration;
using System.Diagnostics;

namespace MyCompany.Diagnostics.Enrichers;

/// <summary>
/// Custom log enricher that adds properties from the diagnostics options
/// </summary>
public class CustomLogEnricher : ILogEventEnricher
{
    private readonly DiagnosticsOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomLogEnricher"/> class
    /// </summary>
    /// <param name="options">The diagnostics options</param>
    public CustomLogEnricher(DiagnosticsOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Enriches the log event with properties from the diagnostics options
    /// </summary>
    /// <param name="logEvent">The log event to enrich</param>
    /// <param name="propertyFactory">The property factory</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // Add service information
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("service", _options.ServiceName));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("environment", _options.Environment));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("version", _options.Version));

        // Add all custom tags
        foreach (var tag in _options.Tags)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(tag.Key, tag.Value));
        }

        // Add trace context if available
        var activity = Activity.Current;
        if (activity != null)
        {
            if (!string.IsNullOrEmpty(activity.TraceId.ToString()))
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("traceId", activity.TraceId.ToString()));
            }
            
            if (!string.IsNullOrEmpty(activity.SpanId.ToString()))
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("spanId", activity.SpanId.ToString()));
            }
            
            if (activity.ParentSpanId != default)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("parentSpanId", activity.ParentSpanId.ToString()));
            }
        }
    }
}