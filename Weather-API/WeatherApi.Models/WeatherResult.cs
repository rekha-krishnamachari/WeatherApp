using System;
namespace WeatherApi.Models
{
    public sealed class WeatherResult
    {
        public string Date { get; init; } = string.Empty; // ISO yyyy-MM-dd
        public double? MinTemperature { get; init; }
        public double? MaxTemperature { get; init; }
        public double? Precipitation { get; init; }
        public string Status { get; init; } = "Ok";
        public string? ErrorMessage { get; init; }
    }
}
