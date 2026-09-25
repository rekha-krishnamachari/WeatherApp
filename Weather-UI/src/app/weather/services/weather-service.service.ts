import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { WeatherResult } from '../model/weather-result';

@Injectable({
  providedIn: 'root'
})
export class WeatherService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/weather`;

  getWeatherData(): Observable<WeatherResult[]> {
    return this.http.get<WeatherResult[]>(this.apiUrl);
  }
}
