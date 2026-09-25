export class WeatherRequest {
  date: string;
  latitude?: number;
  longitude?: number;

  constructor(date: string, latitude?: number, longitude?: number) {
    this.date = date;
    this.latitude = latitude;
    this.longitude = longitude;
  }
}
