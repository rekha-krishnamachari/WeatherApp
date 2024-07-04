import { Component, OnInit } from '@angular/core';
import { WeatherService } from '../service/weather.service';

@Component({
  selector: 'app-weather',
  templateUrl: './weather.component.html',
  styleUrls: ['./weather.component.scss']
})
export class WeatherComponent implements OnInit {

  cities = ['RIO DE JANEIRO', 'BEIJING', 'LOS ANGELES'];
  selectedCity = 'RIO DE JANEIRO';
  hourlyWeather: any[] = [];
  dailyWeather: any[] = [];
  lowestTemp: number | null = null;
  highestTemp: number | null = null;
  weatherData: any[] = [];

  constructor(private weatherService: WeatherService) { }

  ngOnInit(): void {
      this.getWeatherData();
  }

  selectCity(city: string): void {
    this.selectedCity = city;
    this.getWeatherData();
  }

  getWeatherData() {
    this.weatherData = [];
    this.weatherService.getWeatherForecast(this.selectedCity).subscribe(
      (data) => {
        this.processWeatherData(data);
      },
      (error) => {
        console.error('Error fetching weather data', error);
      }
    );

  }


  processWeatherData(data: any): void {

    const currentDate = new Date().toISOString().slice(0, 2);
    const first5Records = data.list.filter((record: any) => {
      return record.dt_txt.includes(currentDate);
    }).slice(0, 5);

    this.hourlyWeather = first5Records.map((record: any) => ({
      time: record.dt_txt,
      temp: Math.trunc(record.main.temp),
      icon:  `https://openweathermap.org/img/w/${record.weather[0].icon}.png`
    }));

    console.log(this.hourlyWeather);

    const daysData: { [key: string]: any[] } = {};

    data.list.forEach((record: any) => {      
      const date = record.dt_txt.split(' ')[0];
      if (!daysData[date]) {
        daysData[date] = [];
      }
      if (daysData[date].length < 8) {
        daysData[date].push(record);
      }
    });    

    for (let date in daysData) {
      const dayRecords = daysData[date];
      let lowestTemp = null;
      let highestTemp = null;
      let description = null;
      let icon = null;

      dayRecords.forEach((record: any) => {
        const temp = record.main.temp;
        if (lowestTemp === null || temp < lowestTemp) {
          lowestTemp = Math.trunc(temp);
        }
        if (highestTemp === null || temp > highestTemp) {
          highestTemp = Math.trunc(temp);
        }
        description = record.weather[0].description;
        icon = `https://openweathermap.org/img/w/${record.weather[0].icon}.png`
      });

      this.weatherData.push({
        date,
        description,
        lowestTemp,
        highestTemp,
        icon
      });
    }
    console.log(this.weatherData);
  }



}
