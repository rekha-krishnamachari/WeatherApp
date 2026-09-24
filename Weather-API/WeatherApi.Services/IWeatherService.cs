using WeatherApi.Models;

namespace WeatherApi.Services
{
    public interface IWeatherService
    {
        Task<WeatherResult> GetWeatherForDateAsync(DateTime date, double latitude, double longitude, CancellationToken cancellationToken = default);
    }
}
