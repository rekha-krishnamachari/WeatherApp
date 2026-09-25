import { Component, OnInit, inject, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WeatherResult } from '../model/weather-result';
import { WeatherService } from '../services/weather-service.service';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';

@Component({
  selector: 'app-weather-list',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatSortModule],
  templateUrl: './weather-list.component.html',
  styleUrl: './weather-list.component.scss'
})
export class WeatherListComponent implements OnInit {
  private weatherService = inject(WeatherService);
  displayedColumns: string[] = ['date', 'minTemperature', 'maxTemperature', 'precipitation'];
  dataSource = new MatTableDataSource<WeatherResult>([]);
  errorMessage: string | null = null;

  @ViewChild(MatSort) sort!: MatSort;

  ngOnInit(): void {
    this.loadWeatherData();
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
  }

  loadWeatherData(): void {
    this.weatherService.getWeatherData().subscribe({
      next: (data: WeatherResult[]) => {
        this.errorMessage = null;
        this.dataSource.data = data || [];
        this.dataSource.sort = this.sort;
      },
      error: (error: any) => {
        this.errorMessage = error?.error?.message || error?.message || 'Something went wrong while loading weather data.';
        console.error('Error fetching weather data:', error);
      }
    });
  }

  sortData(sort: Sort): void {
    this.dataSource.data = this.dataSource.data.slice().sort((a, b) => {
      const isAsc = sort.direction === 'asc';
      switch (sort.active) {
        case 'date':
          return compare(a.date, b.date, isAsc);
        case 'minTemperature':
          return compare(a.minTemperature, b.minTemperature, isAsc);
        case 'maxTemperature':
          return compare(a.maxTemperature, b.maxTemperature, isAsc);
        case 'precipitation':
          return compare(a.precipitation, b.precipitation, isAsc);
        default:
          return 0;
      }
    });
  }
}

function compare(a: number | string, b: number | string, isAsc: boolean): number {
  return (a < b ? -1 : 1) * (isAsc ? 1 : -1);
}
