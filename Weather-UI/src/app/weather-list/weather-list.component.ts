import { Component, OnInit, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiServiceService } from '../services/api-service.service';
import { WeatherResult } from '../interface/weatherResult';

@Component({
  selector: 'app-weather-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './weather-list.component.html',
  styleUrl: './weather-list.component.scss'
})
export class WeatherListComponent implements OnInit {
  private apiService = Inject(ApiServiceService);
  weatherResult: WeatherResult[] = [];
  sortField: keyof WeatherResult = 'date';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor() {}

  ngOnInit(): void {
    this.loadWeatherData();
  }

  loadWeatherData(): void {
    this.apiService.getWeatherData(0, 0, '2023-01-01', '2023-01-31').subscribe({
      next: (data: WeatherResult[]) => {
        this.weatherResult = data;
        this.sortWeatherData();
      },
      error: (error: any) => {
        console.error('Error fetching weather data:', error);
      }
    });
  }

  sortWeatherData(): void {
    const direction = this.sortDirection === 'asc' ? 1 : -1;

    this.weatherResult = [...this.weatherResult].sort((a, b) => {
      const valueA = a[this.sortField];
      const valueB = b[this.sortField];

      if (valueA == null && valueB == null) return 0;
      if (valueA == null) return 1;
      if (valueB == null) return -1;

      if (typeof valueA === 'string' && typeof valueB === 'string') {
        return valueA.localeCompare(valueB) * direction;
      }

      return ((Number(valueA) - Number(valueB)) * direction);
    });
  }

  onSortFieldChange(field: string): void {
    this.sortField = field as keyof WeatherResult;
    this.sortWeatherData();
  }

  toggleSortDirection(): void {
    this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    this.sortWeatherData();
  }
}
