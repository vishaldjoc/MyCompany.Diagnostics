using System;
using OpenTelemetry.Trace;
using OtlpExportProtocol = OpenTelemetry.Exporter.OtlpExportProtocol;
using MyCompany.Diagnostics.Configuration;

namespace MyCompany.Diagnostics.OpenTelemetry;

/// <summary>
/// Configurator for Datadog-specific OpenTelemetry exporter settings
/// </summary>
internal static class DatadogExporterConfigurator
{
    /// <summary>
    /// Configures the OpenTelemetry exporter for Datadog
    /// </summary>
    /// <param name="builder">The trace provider builder</param>
    /// <param name="options">The diagnostics options</param>
    /// <returns>The configured trace provider builder</returns>
    public static TracerProviderBuilder ConfigureDatadogExporter(
        this TracerProviderBuilder builder, 
        DatadogOptions options)
    {
        if (!options.Enabled || string.IsNullOrWhiteSpace(options.ApiKey))
        {
            return builder;
        }

        return builder.AddOtlpExporter(otlpOptions =>
        {
            // Set Datadog-specific headers
            otlpOptions.Headers = $"dd-api-key={options.ApiKey}";
            
            // Set Datadog intake endpoint (if using agentless mode)
            var datadogSite = string.IsNullOrWhiteSpace(options.Site) ? "datadoghq.com" : options.Site;
            otlpOptions.Endpoint = new Uri($"https://trace.{datadogSite}/api/v2/traces");
            
            // Use HTTP/protobuf protocol which is supported by Datadog
            otlpOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
        });
    }
}