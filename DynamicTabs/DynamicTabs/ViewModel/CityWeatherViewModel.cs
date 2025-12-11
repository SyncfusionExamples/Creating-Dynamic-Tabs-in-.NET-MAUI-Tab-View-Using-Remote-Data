using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DynamicTabs
{
    // ViewModel for a single city's weather
    public class CityWeatherViewModel : INotifyPropertyChanged 
    {
        public string Name { get; } // City name
        public double Latitude { get; } // Latitude for API calls
        public double Longitude { get; } // Longitude for API calls

        private string dateText = DateTime.Now.ToString("dddd, MMMM dd", CultureInfo.InvariantCulture); // Initialize date text
        public string DateText { get => dateText; set => Set(ref dateText, value); } // Bindable date text property

        private string conditionText = ""; // Backing field for condition string (e.g., "Cloudy")
        public string ConditionText { get => conditionText; set => Set(ref conditionText, value); } // Bindable condition text

        private string tempText = "--"; // Backing field for temperature string
        public string TempText { get => tempText; set => Set(ref tempText, value); } // Bindable temperature text

        private string icon = "cloudy.png"; // Default icon
        public string Icon { get => icon; set => Set(ref icon, value); } // Bindable icon name

        public ObservableCollection<DailyForecastItem> NextDays { get; } = new(); // Collection for next day's forecast

        private static readonly HttpClient Http = new(); // Shared HttpClient for API calls
        private CancellationTokenSource? cts; // For canceling in-flight requests
        private bool isLoaded; // Flag to indicate the city data has been loaded
        private Task? loadTask; // To prevent duplicate loads

        public CityWeatherViewModel(string name, double lat, double lon)
        {
            Name = name; 
            Latitude = lat; 
            Longitude = lon; 
        }

        // returns immediately if already loaded
        public Task EnsureLoadedAsync()
        {
            if (isLoaded) return Task.CompletedTask; // If already loaded, exit immediately
            return loadTask ??= LoadCoreAsync(); // Start the load if not started
        }

        // Actual load; called once per city 
        private async Task LoadCoreAsync()
        {
            cts?.Cancel(); // Cancel any previous request if still running
            cts = new CancellationTokenSource(); // Create a new cancellation token source
            var token = cts.Token; // Get the token

            try
            {
                // Build API URL for Open-Meteo service
                string apiUrl =
                    "https://api.open-meteo.com/v1/forecast" +
                    $"?latitude={Latitude.ToString(CultureInfo.InvariantCulture)}" + // Use invariant culture for query
                    $"&longitude={Longitude.ToString(CultureInfo.InvariantCulture)}" +
                    "&hourly=temperature_2m,weathercode" + // Request hourly data
                    "&daily=temperature_2m_max,temperature_2m_min,weathercode" + // Request daily data
                    "&timezone=auto"; // Auto timezone

                using var response = await Http.GetAsync(apiUrl, token); // Send HTTP GET with cancellation
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(token); // Read response as string
                var forecast = JsonSerializer.Deserialize<WeatherApiResponse>(json); // Deserialize to model

                if (forecast is null || forecast.Hourly is null || forecast.Daily is null) return; // Validate data presence

                // Extract first hourly temperature (simplified "current" temp)
                double currentTemp = (forecast.Hourly.TemperatureData?.Count ?? 0) > 0
                    ? forecast.Hourly.TemperatureData![0]
                    : double.NaN;

                // Extract first hourly weather code (simplified "current" condition)
                int currentWeather = (forecast.Hourly.Weather?.Count ?? 0) > 0
                    ? forecast.Hourly.Weather![0]
                    : -1;

                var mapping = WeatherMapping.Map(currentWeather); // Map weather code to icon/label

                // Update UI-bound properties on main thread
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    DateText = DateTime.Now.ToString("dddd, MMMM dd", CultureInfo.InvariantCulture); // Refresh date label
                    TempText = double.IsNaN(currentTemp) ? "--" : $"{currentTemp:0}°C"; // Format temp or show placeholder
                    ConditionText = mapping.Label; // Set condition label
                    Icon = mapping.Icon; // Set icon file name

                    NextDays.Clear(); // Reset forecast list

                    var daily = forecast.Daily; // Reference daily data
                                                // Determine min count across parallel arrays to avoid index errors
                    int count = Math.Min(
                        Math.Min(daily.Time?.Count ?? 0, daily.TemperatureMax?.Count ?? 0),
                        Math.Min(daily.TemperatureMin?.Count ?? 0, daily.WeatherData?.Count ?? 0));

                    int start = GetFirstFutureDayIndex(daily.Time!); // Start from the next day after today

                    for (int i = start; i < count; i++) // Iterate through daily data
                    {
                        var dateStr = daily.Time![i]; // Date string from API
                        string dayText = DateTime.TryParse(dateStr, out var d) // Try to parse to DateTime
                            ? $"{d:ddd} • {d:MMM dd}" // Format (e.g., "Tue • Jan 30")
                            : dateStr; // Fallback to original string

                        var mapping = WeatherMapping.Map(daily.WeatherData![i]); // Map daily weather code

                        NextDays.Add(new DailyForecastItem // Add one day's forecast to the list
                        {
                            DayText = dayText, // Display day text
                            HighText = $"High : {daily.TemperatureMax![i]:0}°", // Max temp formatted
                            LowText = $"Low : {daily.TemperatureMin![i]:0}°", // Min temp formatted
                            Icon = mapping.Icon // Daily icon
                        });
                    }
                });

                isLoaded = true; 
            }
            catch (OperationCanceledException) { } // Ignore cancellations
            finally
            {
                // if failed, allow retry on next call
                if (!isLoaded) loadTask = null; // Reset memoized task if not successfully loaded
            }
        }

        // Find the index of the first date after today
        private static int GetFirstFutureDayIndex(System.Collections.Generic.List<string> dates)
        {
            if (dates == null || dates.Count == 0) return 0; // Handle null/empty

            var today = DateTime.Now.Date; // Today's date (no time component)

            for (int i = 0; i < dates.Count; i++) // Iterate dates
                if (DateTime.TryParse(dates[i], out var d) && d.Date > today) // If parse succeeds and date is in the future
                    return i; // Return index of first future day

            return dates.Count > 1 ? 1 : 0; // Fallback: pick the second element if available, else first
        }

        public event PropertyChangedEventHandler? PropertyChanged; // Event for property change notifications

        private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (!Equals(field, value)) // Only update if different
            {
                field = value; // Update backing field
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); // Notify binding system
            }
        }
    }

}
