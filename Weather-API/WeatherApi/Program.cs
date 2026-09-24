using WeatherApi.Models;
using WeatherApi.Services;
using WeatherApi.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<IWeatherService, WeatherService>();  
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/weather", async (IWeatherService weatherService, IWebHostEnvironment env, double latitude, double longitude, CancellationToken cancellationToken) =>
{
    var lat = latitude == 0 ? 32.78 : latitude;
    var lon = longitude == 0 ? -96.8 : longitude;




    var filePath = Path.Combine(env.ContentRootPath, "dates.txt");
    var results = new List<WeatherResult>();
    var fileErrors = new List<string>();

    if (!File.Exists(filePath))
    {
        return Results.NotFound(new { Message = $"dates file not found at path: {filePath}" });
    }

    var (parsedIso, errors) = DateFileReader.ReadAndParseDates(filePath);
    if (errors?.Count > 0) fileErrors.AddRange(errors);

    foreach (var iso in parsedIso)
    {
        if (!DateTime.TryParseExact(iso, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var date))
        {
            fileErrors.Add($"Failed to parse ISO date: {iso}");
            continue;
        }

        var entry = await weatherService.GetWeatherForDateAsync(date, lat, lon, cancellationToken).ConfigureAwait(false);
        results.Add(entry);
    }
    return Results.Ok(new
    {
        Latitude = lat,
        Longitude = lon,
        Results = results,
        FileErrors = fileErrors
    });



})
.Produces(200);

app.Run();
