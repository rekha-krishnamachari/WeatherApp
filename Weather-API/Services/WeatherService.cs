using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WeatherApi.Models;

namespace WeatherApi.Services
{
    public sealed class WeatherService : IWeatherService
    {
        private readonly HttpClient _http;
        private const double DefaultLatitude = 32.78;
        private const double DefaultLongitude = -96.8;

        public WeatherService(HttpClient http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
        }

        public async Task<WeatherResult> GetWeatherForDateAsync(WeatherRequest? request, CancellationToken cancellationToken = default)
        {
            if (request is null || !request.Date.HasValue)
            {
                return new WeatherResult
                {
                    Date = string.Empty,
                    Status = "InvalidRequest",
                    ErrorMessage = "Request.Date is required."
                };
            }

            var iso = request.Date.Value.ToString("yyyy-MM-dd");
            var latitude = request.Latitude ?? DefaultLatitude;
            var longitude = request.Longitude ?? DefaultLongitude;

            // Use relative path; HttpClient.BaseAddress is configured in Program.cs
            var relativePath =
                $"/v1/archive?latitude={latitude}&longitude={longitude}&start_date={iso}&end_date={iso}&daily=temperature_2m_max,temperature_2m_min,precipitation_sum&timezone=UTC";

            try
            {
                using var resp = await _http.GetAsync(relativePath, cancellationToken).ConfigureAwait(false);

                if (!resp.IsSuccessStatusCode)
                {
                    return new WeatherResult
                    {
                        Date = iso,
                        Status = "ApiError",
                        ErrorMessage = $"Open-Meteo responded with {(int)resp.StatusCode} {resp.ReasonPhrase}"
                    };
                }

                var content = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(content))
                {
                    return new WeatherResult
                    {
                        Date = iso,
                        Status = "ApiError",
                        ErrorMessage = "Open-Meteo returned empty response body."
                    };
                }

                JsonDocument doc;
                try
                {
                    doc = JsonDocument.Parse(content);
                }
                catch (JsonException jex)
                {
                    return new WeatherResult
                    {
                        Date = iso,
                        Status = "ApiError",
                        ErrorMessage = $"Failed to parse Open-Meteo JSON response: {jex.Message}"
                    };
                }

                using (doc)
                {
                    if (!doc.RootElement.TryGetProperty("daily", out var daily))
                    {
                        return new WeatherResult
                        {
                            Date = iso,
                            Status = "ApiError",
                            ErrorMessage = "Missing 'daily' section in Open-Meteo response."
                        };
                    }

                    double? min = null, max = null, precip = null;

                    if (daily.TryGetProperty("temperature_2m_min", out var minArr) && minArr.GetArrayLength() > 0)
                    {
                        if (minArr[0].ValueKind == JsonValueKind.Number && minArr[0].TryGetDouble(out var d)) min = d;
                    }

                    if (daily.TryGetProperty("temperature_2m_max", out var maxArr) && maxArr.GetArrayLength() > 0)
                    {
                        if (maxArr[0].ValueKind == JsonValueKind.Number && maxArr[0].TryGetDouble(out var d)) max = d;
                    }

                    if (daily.TryGetProperty("precipitation_sum", out var pArr) && pArr.GetArrayLength() > 0)
                    {
                        if (pArr[0].ValueKind == JsonValueKind.Number && pArr[0].TryGetDouble(out var d)) precip = d;
                    }

                    return new WeatherResult
                    {
                        Date = iso,
                        MinTemperature = min,
                        MaxTemperature = max,
                        Precipitation = precip,
                        Status = "Ok"
                    };
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return new WeatherResult
                {
                    Date = iso,
                    Status = "Cancelled",
                    ErrorMessage = "Request cancelled."
                };
            }
            catch (Exception ex)
            {
                return new WeatherResult
                {
                    Date = iso,
                    Status = "Error",
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}