using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using MyCompany.Diagnostics.Logging.Extension;
using ILogger = Serilog.ILogger;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger _logger;

        public WeatherForecastController(ILogger logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var forecasts = new[] { "Sunny", "Rainy", "Cloudy" };

            // Use LogWithContext
            _logger.LogWithContext(LogEventLevel.Information,
                "Generated {Count} weather forecasts", forecasts.Length);

            // Optional: Use WithTags
            var tags = new Dictionary<string, string>
            {
                { "Module", "Weather" },
                { "Environment", "Development" }
            };

            using (_logger.WithTags(tags))
            {
                _logger.LogWithContext(LogEventLevel.Information,
                    "Forecast tags added for {Count} forecasts", forecasts.Length);
            }

            return Ok(forecasts);
        }
    }
}
