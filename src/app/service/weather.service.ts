import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

const weatherApiKEY = environment.weather_api_KEY;
const weatherLatLongApiURL = environment.weather_lat_long_api_URL;
const weatherCityApiURL=environment.weather_city_api_URL;

@Injectable({
  providedIn: 'root'
})
export class WeatherService {

  private apiURL = 'https://api.openweathermap.org/data/2.5/forecast';
  private apiKEY = '482944e26d320a80bd5e4f23b3de7d1f';

  constructor(private httpClient: HttpClient) { }

  httpOptions = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json',
      'Access-Control-Allow-Origin': '*',
      'Access-Control-Allow-Credentials': 'true',
      'Access-Control-Allow-Headers': 'Content-Type',
      'Access-Control-Allow-Methods': 'GET,PUT,POST,DELETE' 
    }),
    params: new HttpParams({
      
    })
  }

  getWeatherForecast(city: string): Observable<any> {
    const url = `${this.apiURL}?q=${city}&appid=${this.apiKEY}&units=metric`;
    return this.httpClient.get<any>(url);
  }

  // getLatandLong(city: string): any {   
  //   let data = { apiKey: "482944e26d320a80bd5e4f23b3de7d1f" } ;     
  //   return this.httpClient.get<any>(weatherLatLongApiURL + city, { params: data });
  // }

  // getWeather(lat:any, lon:any): Observable<any>{
  //   let data = { apiKey: "482944e26d320a80bd5e4f23b3de7d1f" } ;   
  //   return this.httpClient.get<any>(weatherCityApiURL+  'lat=' + lat  +  '&lon=' + lon +'&exclude={}', { params: data });
  // }

  // getHourlyWeather(city: string): Observable<any> {
  //   return this.httpClient.get<any>(`${this.apiUrl}/hourly?city=${city}`);
  // }

  // getDailyWeather(city: string): Observable<any> {
  //   return this.httpClient.get<any>(`${this.apiUrl}/daily?city=${city}`);
  // }
}
