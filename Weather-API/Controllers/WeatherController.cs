using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using WeatherApi.Models;
using WeatherApi.Services;
using WeatherApi.Utils;

namespace WeatherApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        private readonly IHostEnvironment _env;
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public WeatherController(IWeatherService weatherService, IHostEnvironment env)
        {
            _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        // GET /api/weather?latitude=32.78&longitude=-96.8
        // Returns a list of WeatherResponse entries. For each valid date:
        // - If cached file exists under weather-data/{yyyy-MM-dd}.json, use it.
        // - Otherwise call the service, store the result as JSON and return it.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WeatherResponse>>> Get([FromQuery] WeatherQuery? query = null, CancellationToken cancellationToken = default)
        {
            // Trim incoming values (query may be null)
            var latRaw = query?.Latitude?.Trim();
            var lonRaw = query?.Longitude?.Trim();

            // Resolve coordinates with validation and sensible defaults
            double lat = 32.78; // default Dallas
            double lon = -96.8;

            if (!string.IsNullOrEmpty(latRaw) && double.TryParse(latRaw, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var parsedLat))
            {
                lat = parsedLat;
            }

            if (!string.IsNullOrEmpty(lonRaw) && double.TryParse(lonRaw, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var parsedLon))
            {
                lon = parsedLon;
            }

            var datesFilePath = Path.Combine(_env.ContentRootPath, "dates.txt");
            if (!System.IO.File.Exists(datesFilePath))
            {
                return Ok(Array.Empty<WeatherResponse>());
            }

            // Ensure storage folder exists
            var storageDir = Path.Combine(_env.ContentRootPath, "weather-data");
            Directory.CreateDirectory(storageDir);

            var responses = new List<WeatherResponse>();

            var (parsedIsoDates, errors) = DateFileReader.ReadAndParseDates(datesFilePath);

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

                var storageFile = Path.Combine(storageDir, $"{iso}.json");

                // If cached file exists, attempt to read and return it without calling the API.
                if (System.IO.File.Exists(storageFile))
                {
                    try
                    {
                        var cached = await System.IO.File.ReadAllTextAsync(storageFile, cancellationToken).ConfigureAwait(false);
                        if (!string.IsNullOrWhiteSpace(cached))
                        {
                            var deserialized = JsonSerializer.Deserialize<WeatherResponse>(cached, SerializerOptions);
                            if (deserialized != null)
                            {
                                responses.Add(deserialized);
                                continue; // next date
                            }
                        }
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        responses.Add(new WeatherResponse
                        {
                            Date = iso,
                            Status = "Cancelled",
                            ErrorMessage = "Request cancelled while reading cached file."
                        });
                        continue;
                    }
                    catch (Exception ex)
                    {
                        responses.Add(new WeatherResponse
                        {
                            Date = iso,
                            Status = "CacheReadError",
                            ErrorMessage = $"Failed reading cache: {ex.Message}"
                        });
                        // continue to call API to try to obtain fresh data
                    }
                }

                // Build per-date request using resolved coordinates
                var perDateRequest = new WeatherRequest
                {
                    Date = dt,
                    Latitude = lat,
                    Longitude = lon
                };

                // Call service
                WeatherResult svcResult;
                try
                {
                    svcResult = await _weatherService.GetWeatherForDateAsync(perDateRequest, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    responses.Add(new WeatherResponse
                    {
                        Date = iso,
                        Status = "Cancelled",
                        ErrorMessage = "Request cancelled while calling weather service."
                    });
                    continue;
                }
                catch (Exception ex)
                {
                    responses.Add(new WeatherResponse
                    {
                        Date = iso,
                        Status = "Error",
                        ErrorMessage = $"Unexpected error calling weather service: {ex.Message}"
                    });
                    continue;
                }

                var weatherResp = new WeatherResponse
                {
                    Date = svcResult.Date,
                    MinTemperature = svcResult.MinTemperature,
                    MaxTemperature = svcResult.MaxTemperature,
                    Precipitation = svcResult.Precipitation,
                    Status = svcResult.Status,
                    ErrorMessage = svcResult.ErrorMessage
                };

                try
                {
                    var tempFile = Path.Combine(storageDir, $"{iso}.{Guid.NewGuid():N}.tmp");
                    var serialized = JsonSerializer.Serialize(weatherResp, SerializerOptions);
                    await System.IO.File.WriteAllTextAsync(tempFile, serialized, cancellationToken).ConfigureAwait(false);

                    if (System.IO.File.Exists(storageFile))
                    {
                        System.IO.File.Replace(tempFile, storageFile, null);
                    }
                    else
                    {
                        System.IO.File.Move(tempFile, storageFile);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // If cancellation occurs while writing, report but still return the result obtained.
                    weatherResp = weatherResp with
                    {
                        Status = "Cancelled",
                        ErrorMessage = "Request cancelled while writing cache."
                    };
                }
                catch (Exception ex)
                {
                    // If persisting fails, include cache error but still return the obtained result.
                    weatherResp = new WeatherResponse
                    {
                        Date = weatherResp.Date,
                        MinTemperature = weatherResp.MinTemperature,
                        MaxTemperature = weatherResp.MaxTemperature,
                        Precipitation = weatherResp.Precipitation,
                        Status = weatherResp.Status == "Ok" ? "OkWithCacheError" : weatherResp.Status,
                        ErrorMessage = weatherResp.ErrorMessage is null ? $"Failed to persist cache: {ex.Message}" : $"{weatherResp.ErrorMessage}; Failed to persist cache: {ex.Message}"
                    };
                }

                responses.Add(weatherResp);
            }

            return Ok(responses);
        }
    }
}