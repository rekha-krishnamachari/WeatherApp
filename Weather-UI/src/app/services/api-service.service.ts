import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { WeatherResult } from '../interface/weatherResult';
@Injectable({
  providedIn: 'root'
})
export class ApiServiceService {
  private http=inject(HttpClient);
  private apiUrl = 'https://localhost:44396/api/weather'; 
  
 getWeatherData(latitude:number,longitude:number, startDate:string, endDate:string): Observable<WeatherResult[]> {
  return this.http.get<WeatherResult[]>(`${this.apiUrl}?latitude=${latitude}&longitude=${longitude}&startDate=${startDate}&endDate=${endDate}`)
}
}
