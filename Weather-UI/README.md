# Weather App

This repository contains two parts:

- Angular UI: `Weather-UI`
- .NET backend API: `Weather-API`

The UI calls the backend at `http://localhost:5021/api/weather`.

## Prerequisites

Before running the app, make sure you have:

- Node.js 18+ and npm
- Angular CLI 18
- .NET 8 SDK

## How to run the backend

From the parent folder (`c:\Projects\WeatherApp`):

```bash
dotnet restore .\Weather-API\WeatherApi.sln
cd .\Weather-API\WeatherApi
dotnet run
```

The API should start on:

- `http://localhost:5021`
- Swagger UI: `http://localhost:5021/swagger`

The backend is configured to accept requests from the Angular UI on `http://localhost:4200`.

## How to run the UI

Open a new terminal and go to the UI project folder:

```bash
cd c:\Projects\WeatherApp\Weather-UI
npm install
npm start
```

Or, if you prefer the Angular CLI directly:

```bash
cd c:\Projects\WeatherApp\Weather-UI
ng serve
```

Then open:

- `http://localhost:4200`

## Assumptions

- The backend must be running before the UI loads weather data.
- The UI is expected to call the API at `http://localhost:5021/api/weather`.
- The Angular app runs on port `4200`.
- The backend is configured with CORS to allow requests from `http://localhost:4200`.
- The API is running in development mode and is not secured with authentication.

## Troubleshooting

If the UI shows an error or no data:

1. Verify the backend is running.
2. Check `http://localhost:5021/swagger` to confirm the API is available.
3. Confirm the browser console does not show a CORS issue.
4. Ensure you are using the correct project folder for each app.

## Notes

The backend and UI are separate projects, so they need to be started independently in different terminals.
