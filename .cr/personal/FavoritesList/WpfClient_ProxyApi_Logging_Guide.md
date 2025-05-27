
# 🔁 WPF Client to Proxy API Logging with Serilog and Datadog

This guide explains how to forward telemetry logs from a **WPF application** to a **proxy API**, and from there to **Datadog**, using `Serilog.Formatting.Compact` and `Serilog.Formatting.Compact.Reader`.

---

## 📦 Requirements

### WPF Client
- .NET 6+ or .NET Framework with Serilog support
- Internet access to reach the proxy

### Proxy API
- ASP.NET Core Web API
- Internet access to reach Datadog

---

## 📁 Folder Structure

```
project-root/
├── WpfClient/
└── TelemetryProxyApi/
```

---

## 🖥️ Part 1: WPF Client Setup

### 1. Install NuGet Packages

```bash
dotnet add package Serilog
dotnet add package Serilog.Sinks.Http
dotnet add package Serilog.Formatting.Compact
```

### 2. Configure Serilog

In `App.xaml.cs` or `Main` method:

```csharp
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Http(
        requestUri: "https://yourproxy.com/api/telemetry/log",
        textFormatter: new CompactJsonFormatter()
    )
    .CreateLogger();
```

### 3. Log Example

```csharp
try
{
    throw new InvalidOperationException("Something went wrong!");
}
catch (Exception ex)
{
    Log.Error(ex, "Failed to load user profile {UserId}", "alice123");
}
```

---

## 🌐 Part 2: Proxy API Setup

### 1. Create ASP.NET Core Web API Project

```bash
dotnet new webapi -n TelemetryProxyApi
cd TelemetryProxyApi
```

### 2. Install NuGet Packages

```bash
dotnet add package Serilog
dotnet add package Serilog.Sinks.Datadog.Logs
dotnet add package Serilog.Formatting.Compact.Reader
```

### 3. Configure Logger (in `Program.cs`)

```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.DatadogLogs(
        apiKey: "YOUR_DATADOG_API_KEY",
        service: "wpf-telemetry-proxy",
        source: "proxy-api",
        tags: new[] { "env:prod" })
    .CreateLogger();

builder.Host.UseSerilog();
```

### 4. Create Telemetry Controller

```csharp
using Microsoft.AspNetCore.Mvc;
using Serilog.Events;
using Serilog.Formatting.Compact.Reader;

[ApiController]
[Route("api/telemetry")]
public class TelemetryController : ControllerBase
{
    [HttpPost("log")]
    public async Task<IActionResult> ReceiveLogs()
    {
        using var reader = new StreamReader(Request.Body);
        using var logReader = new LogEventReader(reader);

        while (logReader.TryRead(out LogEvent logEvent))
        {
            Log.Logger.Write(logEvent);
        }

        return Ok();
    }
}
```

---

## ✅ Features Preserved

| Feature           | Supported |
|-------------------|-----------|
| Timestamp         | ✅         |
| Log level         | ✅         |
| Message template  | ✅         |
| Rendered message  | ✅         |
| Exception stack   | ✅ Full    |
| Structured fields | ✅         |

---

## 🔒 Security Note

To prevent abuse:
- Authenticate the proxy API (e.g., using an API key)
- Add request rate limits and validation
- Restrict CORS if applicable

---

## 🧪 Testing

Run the proxy API and the WPF app. Exceptions and logs from the WPF client should appear in **Datadog Logs** under the configured service/source.

---

## 📚 Resources

- [Serilog.Formatting.Compact.Reader](https://github.com/serilog/serilog-formatting-compact-reader)
- [Datadog Serilog Sink](https://github.com/DataDog/serilog-sinks-datadog-logs)
- [Serilog Docs](https://serilog.net)

---

## 👥 Maintainers

- Your Name – your.email@example.com
