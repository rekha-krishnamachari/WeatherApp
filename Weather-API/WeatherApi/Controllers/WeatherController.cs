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
        public async Task<ActionResult<IEnumerable<WeatherResponse>>> Get([FromQuery] WeatherRequest? request = null, CancellationToken cancellationToken = default)
        {
            double? latitude = request?.Latitude;
            double? longitude = request?.Longitude;

            var filePath = Path.Combine(_env.ContentRootPath, "dates.txt");
            if (!System.IO.File.Exists(filePath))
            {
                return Ok(Array.Empty<WeatherResponse>());
            }
            var responses = new List<WeatherResponse>();
            var (parsedIsoDates, errors) = DateFileReader.ReadAndParseDates(filePath);


            foreach (var iso in parsedIsoDates)
            {
                if (!DateTime.TryParseExact(iso, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                {
                    responses.Add(new WeatherResponse
                    {
                        Date = iso,
                        Status = "Invalid",
                        ErrorMessage = "Could not parse normalized ISO date."
                    });
                    continue;
                }

                var perDateRequest = new WeatherRequest
                {
                    Date = dt,
                    Latitude = latitude,
                    Longitude = longitude
                };

                var result = await _weatherService.GetWeatherForDateAsync(perDateRequest, cancellationToken).ConfigureAwait(false);

                responses.Add(new WeatherResponse
                {
                    Date = result.Date,
                    MinTemperature = result.MinTemperature,
                    MaxTemperature = result.MaxTemperature,
                    Precipitation = result.Precipitation,
                    Status = result.Status,
                    ErrorMessage = result.ErrorMessage
                });
            }

            return Ok(responses);
        }
    }
}
