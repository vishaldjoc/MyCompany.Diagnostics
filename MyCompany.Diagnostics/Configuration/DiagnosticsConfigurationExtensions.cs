using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Logging;
using Serilog;
using System;
using ILogger = Serilog.ILogger;

namespace MyCompany.Diagnostics.Configuration;

/// <summary>
/// Extension methods for configuring the diagnostics system
/// </summary>
public static class DiagnosticsConfigurationExtensions
{
    /// <summary>
    /// Configures the diagnostics system for an ASP.NET Core application
    /// </summary>
    /// <param name="builder">The host builder</param>
    /// <param name="configSectionPath">The configuration section path (default: "Diagnostics")</param>
    /// <returns>The host builder for chaining</returns>
    public static IHostBuilder ConfigureDiagnostics(this IHostBuilder builder, string configSectionPath = "Diagnostics")
    {
        return builder.ConfigureServices((context, services) =>
        {
            // Read the configuration section
            var diagnosticsConfig = context.Configuration.GetSection(configSectionPath);
            var options = new DiagnosticsOptions();
            diagnosticsConfig.Bind(options);

            // Add the options to DI container
            services.Configure<DiagnosticsOptions>(diagnosticsConfig);

            // Configure the diagnostics system
            DiagnosticsConfigurator.ConfigureServices(services, options, context.Configuration);
        })
        .UseSerilog((context, serviceProvider, loggerConfig) =>
        {
            var options = new DiagnosticsOptions();
            context.Configuration.GetSection(configSectionPath).Bind(options);
            DiagnosticsConfigurator.ConfigureSerilog(loggerConfig, options, context.Configuration, serviceProvider);
        });
    }

    /// <summary>
    /// Configures the diagnostics system for an ASP.NET Core application
    /// </summary>
    /// <param name="builder">The web application builder</param>
    /// <param name="configSectionPath">The configuration section path (default: "Diagnostics")</param>
    /// <returns>The web application builder for chaining</returns>
    public static IHostApplicationBuilder ConfigureDiagnostics(this IHostApplicationBuilder builder, string configSectionPath = "Diagnostics")
    {
        // Read the configuration section
        var diagnosticsConfig = builder.Configuration.GetSection(configSectionPath);
        var options = new DiagnosticsOptions();
        diagnosticsConfig.Bind(options);

        // Add the options to DI container
        builder.Services.Configure<DiagnosticsOptions>(diagnosticsConfig);

        // Configure the diagnostics system
        DiagnosticsConfigurator.ConfigureServices(builder.Services, options, builder.Configuration);

        // Configure Serilog
        builder.Logging.ClearProviders();
        
        // Create and configure logger
        var logger = new LoggerConfiguration();
        DiagnosticsConfigurator.ConfigureSerilog(logger, options, builder.Configuration, builder.Services.BuildServiceProvider());
        
        // Set as default logger
        Log.Logger = logger.CreateLogger();
        builder.Logging.AddSerilog(Log.Logger);

        return builder;
    }

    /// <summary>
    /// Provides a simplified method to configure diagnostics with the specified configuration
    /// </summary>
    /// <param name="configuration">The configuration</param>
    /// <param name="serviceName">Optional service name override</param>
    /// <returns>The configured logger</returns>
    public static ILogger ConfigureLoggingAndTracing(this IConfiguration configuration, string? serviceName = null)
    {
        // Read the configuration section
        var diagnosticsConfig = configuration.GetSection("Diagnostics");
        var options = new DiagnosticsOptions();
        diagnosticsConfig.Bind(options);

        // Override service name if provided
        if (!string.IsNullOrEmpty(serviceName))
        {
            options.ServiceName = serviceName;
        }

        // Configure Serilog
        var logger = new LoggerConfiguration();
        DiagnosticsConfigurator.ConfigureSerilog(logger, options, configuration, null);
        
        // Set as default logger
        Log.Logger = logger.CreateLogger();

        return Log.Logger;
    }
}