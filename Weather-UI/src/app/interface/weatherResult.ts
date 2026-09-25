export interface WeatherResult {
    date: string;
    minTemperature: number;
    maxTemperatureF: number;
    precipitation: number;
    errorMessage?: string;
    status?: string;
}
