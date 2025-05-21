using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using MyCompany.Diagnostics.Configuration;
using MyCompany.Diagnostics.Enrichers;
using OtlpExportProtocol = OpenTelemetry.Exporter.OtlpExportProtocol;

namespace MyCompany.Diagnostics;

/// <summary>
/// Main configurator for the diagnostics system
/// </summary>
public static class DiagnosticsConfigurator
{
    /// <summary>
    /// Configures services for the diagnostics system
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="options">The diagnostics options</param>
    /// <param name="configuration">The configuration</param>
    public static void ConfigureServices(IServiceCollection services, DiagnosticsOptions options, IConfiguration configuration)
    {
        // Add HttpContextAccessor for correlation ID support
        services.AddHttpContextAccessor();

        // Configure OpenTelemetry if enabled
        if (options.EnableTracing)
        {
            ConfigureOpenTelemetry(services, options, configuration);
        }
    }

    /// <summary>
    /// Configures OpenTelemetry tracing
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="options">The diagnostics options</param>
    /// <param name="configuration">The configuration</param>
    public static void ConfigureOpenTelemetry(IServiceCollection services, DiagnosticsOptions options, IConfiguration configuration)
    {
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                // Configure resource
                builder.SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(options.ServiceName, serviceVersion: options.Version)
                    .AddAttributes(ConvertTagsToAttributes(options.Tags))
                    .AddEnvironmentVariableDetector());

                // Add ASP.NET Core and HTTP client instrumentation
                builder.AddAspNetCoreInstrumentation();
                builder.AddHttpClientInstrumentation();

                // Add console exporter for development
                if (options.EnableConsoleLogging)
                {
                    builder.AddConsoleExporter();
                }

                // Add OTLP exporter for Datadog if enabled
                if (options.Datadog.Enabled)
                {
                    // Configure OTLP exporter for Datadog
                    builder.AddOtlpExporter(otlpOptions =>
                    {
                        // Set Datadog-specific headers
                        otlpOptions.Headers = $"dd-api-key={options.Datadog.ApiKey}";
                        
                        // Set Datadog intake endpoint (if using agentless mode)
                        otlpOptions.Endpoint = new Uri($"https://trace.{options.Datadog.Site}/api/v2/traces");
                        
                        // Use HTTP/protobuf protocol
                        otlpOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
                    });
                }
            });
    }

    /// <summary>
    /// Configures Serilog with the specified options
    /// </summary>
    /// <param name="loggerConfig">The logger configuration</param>
    /// <param name="options">The diagnostics options</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="serviceProvider">The service provider (optional)</param>
    public static void ConfigureSerilog(LoggerConfiguration loggerConfig, DiagnosticsOptions options, IConfiguration configuration, IServiceProvider? serviceProvider)
    {
        // Parse minimum log level
        Enum.TryParse<LogEventLevel>(options.MinimumLogLevel, true, out var minimumLevel);

        // Start with minimum level
        loggerConfig
            .MinimumLevel.Is(minimumLevel)
            
            // Add enrichers
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProcessId()
            .Enrich.WithProcessName()
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .Enrich.WithCorrelationId()
            
            // Add custom enricher with configuration options
            .Enrich.With(new CustomLogEnricher(options));

        // Add console sink if enabled
        if (options.EnableConsoleLogging)
        {
            loggerConfig.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
        }

        // Add Datadog sink if enabled
        if (options.Datadog.Enabled)
        {
            // Use OpenTelemetry sink to send logs to Datadog
            loggerConfig.WriteTo.OpenTelemetry(openTelemetryOptions =>
            {
                openTelemetryOptions.Endpoint = options.Datadog.Endpoint;
                openTelemetryOptions.Headers.Add("DD-API-KEY", options.Datadog.ApiKey);
                openTelemetryOptions.ResourceAttributes.Add("service.name", options.ServiceName);
                openTelemetryOptions.ResourceAttributes.Add("service.version", options.Version);
                openTelemetryOptions.ResourceAttributes.Add("deployment.environment", options.Environment);
                
                // Add custom tags
                foreach (var tag in options.Tags)
                {
                    openTelemetryOptions.ResourceAttributes.Add(tag.Key, tag.Value);
                }
            });
        }

        // Add other sinks and enrichers based on configuration
        loggerConfig.ReadFrom.Configuration(configuration, sectionName: "Serilog");
    }

    /// <summary>
    /// Converts dictionary of tags to KeyValuePair collection for OpenTelemetry
    /// </summary>
    private static IEnumerable<KeyValuePair<string, object>> ConvertTagsToAttributes(Dictionary<string, string> tags)
    {
        foreach (var tag in tags)
        {
            yield return new KeyValuePair<string, object>(tag.Key, tag.Value);
        }
    }
}