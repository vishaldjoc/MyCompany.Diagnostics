# MyCompany.Diagnostics

A comprehensive diagnostics library for .NET applications that integrates Serilog with OpenTelemetry and Datadog.

## Features

- Centralized logging configuration with Serilog
- OpenTelemetry tracing integration
- Datadog agentless export for logs and traces
- Custom log enrichment with organizational context
- Activity-based correlation between logs and traces
- Easy integration with any .NET application

## Installation

Add a reference to the MyCompany.Diagnostics project in your solution:

```xml
<ProjectReference Include="..\MyCompany.Diagnostics\MyCompany.Diagnostics.csproj" />
```

## Configuration

Add the diagnostics configuration section to your appsettings.json:

```json
{
  "Diagnostics": {
    "ServiceName": "YourServiceName",
    "Environment": "Development",
    "Version": "1.0.0",
    "MinimumLogLevel": "Information",
    "EnableConsoleLogging": true,
    "EnableTracing": true,
    "Datadog": {
      "ApiKey": "your_api_key",
      "Site": "datadoghq.com",
      "Endpoint": "https://http-intake.logs.datadoghq.com/api/v2/logs",
      "Enabled": true
    },
    "Tags": {
      "Team": "YourTeam",
      "Project": "YourProject"
    }
  }
}
```

## Usage

### In ASP.NET Core (Program.cs)

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configure diagnostics
builder.Host.ConfigureDiagnostics();

// Add services
builder.Services.AddControllers();

var app = builder.Build();

// Add Serilog request logging
app.UseSerilogRequestLogging();

app.MapControllers();
app.Run();
```

### In a Controller

```csharp
[ApiController]
[Route("[controller]")]
public class ExampleController : ControllerBase
{
    private readonly ILogger<ExampleController> _logger;

    public ExampleController(ILogger<ExampleController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Processing request");
        
        // Add custom context
        using (_logger.WithTags(new Dictionary<string, string> 
        {
            { "endpoint", "GetExample" },
            { "userId", User?.Identity?.Name ?? "anonymous" }
        }))
        {
            // Your code here
            
            // Log with activity context
            _logger.LogWithContext(LogEventLevel.Information, 
                "Operation completed successfully");
        }
        
        return Ok(new { message = "Success" });
    }
}
```

## Environment Variables

All configuration options can be overridden using environment variables with the `DIAGNOSTICS_` prefix:

- `DIAGNOSTICS__SERVICENAME`
- `DIAGNOSTICS__ENVIRONMENT`
- `DIAGNOSTICS__VERSION`
- `DIAGNOSTICS__MINIMUMLOGLEVEL`
- `DIAGNOSTICS__DATADOG__APIKEY`
- `DIAGNOSTICS__DATADOG__ENABLED`

## Advanced Usage

### Custom Enrichers

You can add your own custom log enrichers by extending the Serilog configuration:

```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.With(new YourCustomEnricher())
    .CreateLogger();
```

### Manual Configuration

If you need more control over the configuration:

```csharp
var options = new DiagnosticsOptions
{
    ServiceName = "CustomService",
    Environment = "Production",
    // Other properties
};

var logger = new LoggerConfiguration();
DiagnosticsConfigurator.ConfigureSerilog(logger, options, configuration, serviceProvider);
Log.Logger = logger.CreateLogger();
```