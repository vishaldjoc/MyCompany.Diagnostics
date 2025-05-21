using System.Collections.Generic;

namespace MyCompany.Diagnostics.Configuration;

/// <summary>
/// Represents the configuration options for the diagnostics system
/// </summary>
public class DiagnosticsOptions
{
    /// <summary>
    /// The service name used for logs and traces
    /// </summary>
    public string ServiceName { get; set; } = "Service";

    /// <summary>
    /// The environment (e.g., Development, Staging, Production)
    /// </summary>
    public string Environment { get; set; } = "Development";

    /// <summary>
    /// The version of the service
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Datadog specific configuration
    /// </summary>
    public DatadogOptions Datadog { get; set; } = new();

    /// <summary>
    /// Additional tags to be added to logs and traces
    /// </summary>
    public Dictionary<string, string> Tags { get; set; } = new();

    /// <summary>
    /// Minimum log level for the application
    /// </summary>
    public string MinimumLogLevel { get; set; } = "Information";

    /// <summary>
    /// Whether to include sensitive data in logs. Should be false in production.
    /// </summary>
    public bool IncludeSensitiveData { get; set; } = false;

    /// <summary>
    /// Whether to output logs to the console
    /// </summary>
    public bool EnableConsoleLogging { get; set; } = true;

    /// <summary>
    /// Whether to enable OpenTelemetry tracing
    /// </summary>
    public bool EnableTracing { get; set; } = true;
}

/// <summary>
/// Datadog specific configuration options
/// </summary>
public class DatadogOptions
{
    /// <summary>
    /// Datadog API key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Datadog site (e.g., datadoghq.com, datadoghq.eu)
    /// </summary>
    public string Site { get; set; } = "datadoghq.com";
    
    /// <summary>
    /// Datadog endpoint for logs
    /// </summary>
    public string Endpoint { get; set; } = "https://http-intake.logs.datadoghq.com/api/v2/logs";
    
    /// <summary>
    /// Whether to enable Datadog export
    /// </summary>
    public bool Enabled { get; set; } = false;
}