export interface WeatherResult {
    date: string;
    minTemperature: number;
    maxTemperature: number;
    precipitation: number;
    errorMessage?: string;
    status?: string;
}
