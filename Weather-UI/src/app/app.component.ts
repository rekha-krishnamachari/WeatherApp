import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { WeatherListComponent } from './weather/weather-list/weather-list.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, WeatherListComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'WeatherApp';
}
