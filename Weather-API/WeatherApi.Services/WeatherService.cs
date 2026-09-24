using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WeatherApi.Models;

namespace WeatherApi.Services
{
    public sealed  class WeatherService :IWeatherService
    {
        private readonly HttpClient _httpClient;

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WeatherResult> GetWeatherForDateAsync(DateTime date, double latitude, double longitude, CancellationToken cancellationToken = default)
        {
           var iso=date.ToString("yyyy-MM-dd");
           var url = $"https://archive-api.open-meteo.com/v1/archive?latitude={latitude}&longitude={longitude}&start_date={iso}&end_date={iso}&daily=temperature_2m_max,temperature_2m_min,precipitation_sum&timezone=UTC";

            try
            {
                using var response = await _httpClient.GetAsync(url, cancellationToken);
                if(!response.IsSuccessStatusCode)
                {
                    return new WeatherResult
                    {
                        Date = iso,
                        Status = "Error",
                        ErrorMessage = $"API request failed with status code: {response.StatusCode}"
                    };
                }

                var contentStream=await response.Content.ReadAsStreamAsync(cancellationToken);
                using var doc = await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken);
              
                if(!doc.RootElement.TryGetProperty("daily",out var daily))
                {

                    return new WeatherResult
                    {
                        Date=iso,
                        Status = "Error",
                        ErrorMessage = "Missing 'daily' property in API response"
                    };  

                }
                double? min=null,max=null,precipitation=null;

                if (daily.TryGetProperty("temperature_2m_min", out var minArr)&& minArr.GetArrayLength() > 0)
                {
                    if (minArr[0].ValueKind == JsonValueKind.Number)
                    {
                        min = minArr[0].GetDouble();
                    }
                }

                if (daily.TryGetProperty("temperature_2m_max", out var maxArr) && maxArr.GetArrayLength() > 0)
                {
                    if (maxArr[0].ValueKind == JsonValueKind.Number && maxArr[0].TryGetDouble(out var d)) max = d;
                }

                if (daily.TryGetProperty("precipitation_sum", out var pArr) && pArr.GetArrayLength() > 0)
                {
                    if (pArr[0].ValueKind == JsonValueKind.Number && pArr[0].TryGetDouble(out var d)) precipitation = d;
                }

                return new WeatherResult
                {
                    Date = iso,
                    MinTemperature = min,
                    MaxTemperature = max,
                    Precipitation = precipitation,
                    Status = "Ok"
                };
            }
            catch(Exception ex)
            {
                return new WeatherResult
                {
                    Date = iso,
                    Status = "Error",
                    ErrorMessage = $"Exception occurred: {ex.Message}"
                };
            }
        }
    }
}
