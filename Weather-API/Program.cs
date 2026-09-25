using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeatherApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Load Open-Meteo base URL from configuration (appsettings.json, env vars, etc.)
var openMeteoBase = builder.Configuration.GetValue<string>("OpenMeteo:BaseUrl") ??
                     "https://archive-api.open-meteo.com";

// Register a typed HttpClient for WeatherService with BaseAddress set from configuration.
builder.Services.AddControllers();
builder.Services.AddHttpClient<IWeatherService, WeatherService>(client =>
{
    client.BaseAddress = new Uri(openMeteoBase);
    client.Timeout = TimeSpan.FromSeconds(30); // sensible default
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();