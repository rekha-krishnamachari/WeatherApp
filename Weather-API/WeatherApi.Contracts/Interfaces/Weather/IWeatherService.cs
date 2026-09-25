using WeatherApi.Models.Dto.Weather;

namespace WeatherApi.Contracts.Interfaces.Weather
{
    public interface IWeatherService
    {
        Task<WeatherResponse> GetWeatherForDateAsync(WeatherRequest? weatherRequest, CancellationToken cancellationToken = default);
    }
}
