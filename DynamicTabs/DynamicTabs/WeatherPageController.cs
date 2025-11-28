using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Syncfusion.Maui.TabView;

namespace DynamicTabs
{
    public class WeatherPageController
    {
        private readonly Label dateLabel;
        private readonly Label tempLabel;
        private readonly Label conditionLabel;
        private readonly Image weatherIcon;
        private readonly Layout nextDaysLayout;           
        private readonly SfTabView locationTabView;
        private readonly Func<string, string, string, Task> displayAlertAsync;

        private static readonly HttpClient SharedHttpClient = new();

        private readonly Dictionary<string, (double Latitude, double Longitude)> CityCoordinates = new()
        {
            { "Phoenix",        (33.45, -112.07) },
            { "Seattle",        (47.61, -122.33) },
            { "San Francisco",  (37.77, -122.42) },
            { "Miami",          (25.76, -80.19)  },
            { "Denver",         (39.74, -104.99) },
            { "Chicago",        (41.88, -87.63)  },
            { "New York",       (40.71, -74.01)  },
        };

        private CancellationTokenSource? fetchCancellationSource;
        private readonly Task initialLoadTask;

        public WeatherPageController(
            Label dateLabel,
            Label tempLabel,
            Label conditionLabel,
            Image weatherIcon,
            Layout nextDaysLayout, 
            SfTabView locationTabView,
            Func<string, string, string, Task> displayAlertAsync)
            {
                this.dateLabel = dateLabel;
                this.tempLabel = tempLabel;
                this.conditionLabel = conditionLabel;
                this.weatherIcon = weatherIcon;
                this.nextDaysLayout = nextDaysLayout;
                this.locationTabView = locationTabView;
                this.displayAlertAsync = displayAlertAsync;

                this.dateLabel.Text = DateTime.Now.ToString("dddd, MMMM dd");

                if (locationTabView.Items.Count > 0)
                    locationTabView.SelectedIndex = 0;

                initialLoadTask = LoadForSelectedTabAsync();
                locationTabView.SelectionChanged += OnLocationSelectionChanged!;
            }

        private async void OnLocationSelectionChanged(object? sender, EventArgs args)
        {
            await LoadForSelectedTabAsync();
        }

        public async Task LoadForSelectedTabAsync()
        {
            if (!TryGetSelectedCoordinates(out var coordinates))
                return;

            fetchCancellationSource?.Cancel();
            fetchCancellationSource = new CancellationTokenSource();

            try
            {
                await LoadWeatherDataAsync(coordinates, fetchCancellationSource.Token);
            }
            catch (OperationCanceledException)
            {
                
            }
            catch (Exception ex)
            {
                await displayAlertAsync("Error", $"Failed to load weather: {ex.Message}", "OK");
            }
        }

        private bool TryGetSelectedCoordinates(out (double Latitude, double Longitude) coordinates)
        {
            coordinates = default;

            int selectedIndex = (int)locationTabView.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex >= locationTabView.Items.Count)
                return false;

            var tabItem = locationTabView.Items[selectedIndex] as SfTabItem;
            string headerText = tabItem?.Header! as string;

            if (string.IsNullOrWhiteSpace(headerText))
                return false;

            return CityCoordinates.TryGetValue(headerText, out coordinates);
        }

        private async Task LoadWeatherDataAsync((double Latitude, double Longitude) coordinates, CancellationToken cancellationToken)
        {
            string apiUrl =
                $"https://api.open-meteo.com/v1/forecast" +
                $"?latitude={coordinates.Latitude}&longitude={coordinates.Longitude}" +
                $"&hourly=temperature_2m,weathercode" +
                $"&daily=temperature_2m_max,temperature_2m_min,weathercode" +
                $"&timezone=auto";

            string jsonText = await SharedHttpClient.GetStringAsync(apiUrl, cancellationToken);
            var forecast = JsonSerializer.Deserialize<WeatherApiResponse>(jsonText);

            if (forecast is null || forecast.Hourly is null || forecast.Daily is null)
                return;

            double currentTemperature = (forecast.Hourly.TemperatureData?.Count ?? 0) > 0
                ? forecast.Hourly.TemperatureData![0]
                : double.NaN;

            int currentWeather = (forecast.Hourly.Weather?.Count ?? 0) > 0
                ? forecast.Hourly.Weather![0]
                : -1;

            var mapping = MapWeatherCondition(currentWeather);
            tempLabel.Text = double.IsNaN(currentTemperature) ? "--" : $"{currentTemperature:0}°C";
            conditionLabel.Text = mapping.Label;
            weatherIcon.Source = mapping.Icon;

            BuildDailyBlocks(forecast);
        }

        private void BuildDailyBlocks(WeatherApiResponse forecast)
        {
            nextDaysLayout.Children.Clear();

            var daily = forecast.Daily;

            int dayCount = Math.Min(
                Math.Min(daily!.Time?.Count ?? 0, daily.TemperatureMax?.Count ?? 0),
                Math.Min(daily.TemperatureMin?.Count ?? 0, daily.WeatherData?.Count ?? 0)
            );

            if (dayCount <= 0)
                return;

            int startIndex = GetFirstFutureDayIndex(daily.Time!);
            if (startIndex < 0 || startIndex >= dayCount)
                return;

            for (int dayIndex = startIndex; dayIndex < dayCount; dayIndex++)
            {
                DateTime? date = TryParseDate(daily.Time![dayIndex]);
                string weekdayAndDate = date.HasValue
                    ? $"{date.Value:ddd} • {date.Value:MMM dd}"
                    : GetWeekdayAbbreviation(daily.Time[dayIndex]);

                int dailyWeather = daily.WeatherData![dayIndex];
                var mapping = MapWeatherCondition(dailyWeather);
                string dailyIcon = mapping.Icon;

                var block = new VerticalStackLayout
                {
                    Spacing = 6,
                    HorizontalOptions = LayoutOptions.Center,
                    WidthRequest = 86
                };

                block.Children.Add(new Image
                {
                    Source = dailyIcon,
                    HeightRequest = 30,
                    HorizontalOptions = LayoutOptions.Center
                });

                block.Children.Add(new Label
                {
                    Text = weekdayAndDate,
                    TextColor = Colors.White,
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center
                });

                block.Children.Add(new Label
                {
                    Text = $"High : {daily.TemperatureMax![dayIndex]:0}°",
                    TextColor = Colors.White,
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Center
                });

                block.Children.Add(new Label
                {
                    Text = $"Low : {daily.TemperatureMin![dayIndex]:0}°",
                    TextColor = Colors.White,
                    FontSize = 16,
                    Opacity = 0.9,
                    HorizontalOptions = LayoutOptions.Center
                });

                nextDaysLayout.Children.Add(block);
            }
        }

        private static int GetFirstFutureDayIndex(List<string> dates)
        {
            if (dates == null || dates.Count == 0)
                return -1;

            DateTime todayLocal = DateTime.Now.Date;
            for (int i = 0; i < dates.Count; i++)
            {
                if (DateTime.TryParse(dates[i], out var d) && d.Date > todayLocal)
                    return i;
            }
            return dates.Count > 1 ? 1 : -1;
        }

        private static DateTime? TryParseDate(string dateString)
            => DateTime.TryParse(dateString, out var d) ? d : null;

        private static string GetWeekdayAbbreviation(string dateString)
            => DateTime.TryParse(dateString, out var d) ? d.ToString("ddd") : dateString;

        private static (string Icon, string Label) MapWeatherCondition(int weatherValue)
            => weatherValue switch
            {
                0 => ("sunny.png", "Clear"),
                1 or 2 => ("partly_cloudy.png", "Partly cloudy"),
                3 => ("cloudy.png", "Cloudy"),
                45 or 48 => ("fog.png", "Fog"),
                51 or 53 or 55 => ("drizzle.png", "Drizzle"),
                56 or 57 => ("drizzle.png", "Freezing drizzle"),
                61 or 63 or 65 => ("rain.png", "Rain"),
                66 or 67 => ("rain.png", "Freezing rain"),
                71 or 73 or 75 => ("snow.png", "Snow"),
                77 => ("snow.png", "Snow grains"),
                80 or 81 or 82 => ("rain.png", "Rain showers"),
                85 or 86 => ("snow.png", "Snow showers"),
                95 => ("storm.png", "Thunderstorm"),
                96 or 97 => ("storm.png", "Thunderstorm with hail"),
                _ => ("cloudy.png", "Unknown")
            };
    }
}