using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using WeatherApi.Contracts.Interfaces.Weather;
using WeatherApi.Models.Dto.Weather;
using WeatherApi.Utils.Shared;

namespace WeatherApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        private readonly IHostEnvironment _env;

        public WeatherController(IWeatherService weatherService, IHostEnvironment env)
        {
            _weatherService = weatherService;
            _env = env;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] WeatherRequest request, CancellationToken cancellationToken)
        {

            double? latitude = request?.Latitude;
            double? longitude = request?.Longitude;

            var filePath = Path.Combine(_env.ContentRootPath, "dates.txt");
            if (!System.IO.File.Exists(filePath))
                return NotFound(new { Message = $"dates file not found at path: {filePath}" });

            var results = new List<WeatherResponse>();
            var fileErrors = new List<string>();

            var (parsedIsoDates, errors) = DateFileReader.ReadAndParseDates(filePath);
            if (errors is { Count: > 0 })
                fileErrors.AddRange(errors);

            foreach (var iso in parsedIsoDates)
            {
                if (!DateTime.TryParseExact(iso, "yyyy-MM-dd", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dt))
                {
                    fileErrors.Add($"Could not parse ISO date '{iso}'.");
                    continue;
                }

                var req = new WeatherRequest
                {
                    Date = dt,
                    Latitude = latitude,
                    Longitude = longitude
                };

                var entry = await _weatherService.GetWeatherForDateAsync(req, cancellationToken).ConfigureAwait(false);
                results.Add(entry);
            }

            return Ok(new
            {
                Latitude = latitude,
                Longitude = longitude,
                Results = results,
                FileErrors = fileErrors
            });
        }
    }
}
