// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
  production: false,
  weather_api_KEY: '482944e26d320a80bd5e4f23b3de7d1f',
  weather_lat_long_api_URL: 'https://api.openweathermap.org/data/2.5/weather?q=',
  weather_city_api_URL: 'https://api.openweathermap.org/data/2.5/forecast?',
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/dist/zone-error';  // Included with Angular CLI.
