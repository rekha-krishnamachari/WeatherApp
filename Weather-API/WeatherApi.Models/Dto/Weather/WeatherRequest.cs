namespace WeatherApi.Models.Dto.Weather
{
    public class WeatherRequest
    {
        public DateTime? Date { get; init; } 
        public double? Latitude { get; init; }
        public double? Longitude { get; init; }        
    }
}
