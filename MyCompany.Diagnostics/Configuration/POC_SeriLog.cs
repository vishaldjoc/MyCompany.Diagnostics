using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Serilog;
using Serilog.Sinks.Datadog.Logs;

const string datadogApiKey = "";
const string datadogUrl = "https://http-intake.logs.us5.datadoghq.com/v1/input";//"https://http-intake.logs.datadoghq.com/v1/input";
//https://http-intake.logs.us5.datadoghq.com/v1/input telemetry 
for (int i = 0; i < 10; i++)
{
    // ------------------------------
    // OPTION 1: Manual using HttpClient
    // ------------------------------
    await SendLogUsingHttpClient(i);
    // ------------------------------
    // OPTION 2: Structured using Serilog
    // ------------------------------
    LogToDatadogWithSerilog(i);

}






Console.WriteLine("Logs sent. Press any key to exit...");
Console.ReadKey();


// ==============================
// Method 1: HttpClient Approach
// ==============================
static async Task SendLogUsingHttpClient(int i)
{
    var log = new
    {
        message = i.ToString()+":Manual log sent from console app",
        ddsource = "console-http",
        service = "manual-console-service",
        hostname = Environment.MachineName,
        status = "info"
    };

    var json = JsonSerializer.Serialize(log);
    using var httpClient = new HttpClient();

    httpClient.DefaultRequestHeaders.Add("DD-API-KEY", datadogApiKey);
    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    var content = new StringContent(json, Encoding.UTF8, "application/json");
    var response = await httpClient.PostAsync(datadogUrl, content);

    Console.WriteLine(response.IsSuccessStatusCode
        ? "✅ Log sent via HttpClient"
        : $"❌ Failed to send via HttpClient: {response.StatusCode}");
}

// ==============================
// Method 2: Serilog Approach
// ==============================
static void LogToDatadogWithSerilog(int i)
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.DatadogLogs(
            apiKey: datadogApiKey,
            source: "serilog-console",
            service: "serilog-console-service",
            host: Environment.MachineName,
            tags: new[] { "env:dev", "type:console" },
            configuration: new DatadogConfiguration
            {
                Url = datadogUrl
            })
        .CreateLogger();

    Log.Information(i.ToString() + "Serilog structured log from console app.");
    Log.CloseAndFlush();

    Console.WriteLine("✅ Log sent via Serilog");
}
