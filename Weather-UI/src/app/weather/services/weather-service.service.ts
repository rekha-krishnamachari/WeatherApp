import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { WeatherResult } from '../model/weather-result';

@Injectable({
  providedIn: 'root'
})
export class WeatherService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5021/api/weather';

  getWeatherData(): Observable<WeatherResult[]> {
    return this.http.get<WeatherResult[]>(
      `${this.apiUrl}`
    );
  }
}
