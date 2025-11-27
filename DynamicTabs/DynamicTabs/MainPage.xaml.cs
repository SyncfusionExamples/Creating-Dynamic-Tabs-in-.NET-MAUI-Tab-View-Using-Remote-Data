using Syncfusion.Maui.TabView;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DynamicTabs
{
    public partial class MainPage : ContentPage
    {
        private static readonly HttpClient SharedHttpClient = new();  // HttpClient for all web requests

        private readonly Dictionary<string, (double Latitude, double Longitude)> CityCoordinates = new() // Map city names to coordinates
        {
            { "Phoenix",        (33.45, -112.07) },
            { "Seattle",        (47.61, -122.33) },
            { "San Francisco",  (37.77, -122.42) },
            { "Miami",          (25.76, -80.19)  },
            { "Denver",         (39.74, -104.99) },
            { "Chicago",        (41.88, -87.63)  },
            { "New York",       (40.71, -74.01)  },
        };

        private CancellationTokenSource? fetchCancellationSource;      // Used to cancel an active fetch when switching tabs
        private readonly Task InitialLoadTask;                                  // Keeps a reference to the initial load operation
        public MainPage()
        {
            InitializeComponent();

            DateLabel.Text = DateTime.Now.ToString("dddd, MMMM dd");  // Sets the date label to a friendly format

            if (LocationTabView.Items.Count > 0)                       // If there are any city tabs available
            {
                LocationTabView.SelectedIndex = 0;                     // Select the first tab by default
            }
            InitialLoadTask = LoadForSelectedTabAsync();               // Begin loading weather for the initially selected tab (without awaiting)

            LocationTabView.SelectionChanged += OnLocationSelectionChanged!; // handler to react to tab (city) changes
        }

        private async void OnLocationSelectionChanged(object sender, EventArgs args) // Event handler called when the selected tab changes
        {
            await LoadForSelectedTabAsync();                           // Load weather for the new selection
        }

        private async Task LoadForSelectedTabAsync()                   // Set up loading weather for the current tab selection
        {
            if (!TryGetSelectedCoordinates(out var coordinates))       // Get the selected city's coordinates; stop if not found/invalid
            {
                return;
            }
            fetchCancellationSource?.Cancel();                         // Cancel any in-progress fetch (user may have switched quickly)
            fetchCancellationSource = new CancellationTokenSource();   // Create a new cancellation source for this request

            try
            {
                await LoadWeatherDataAsync(coordinates, fetchCancellationSource.Token); // Fetch and display weather with cancellation support
            }
            catch (OperationCanceledException)                         // Expected when a request is canceled due to rapid tab switching
            {

            }
            catch (Exception ex)                                       // unexpected failure
            {
                await DisplayAlertAsync("Error", $"Failed to load weather: {ex.Message}", "OK");
            }
        }

        private bool TryGetSelectedCoordinates(out (double Latitude, double Longitude) coordinates) // Safely resolve selected tab to coordinates
        {
            coordinates = default;                                     // Initialize the out parameter

            int selectedIndex = (int)LocationTabView.SelectedIndex;    // Read the current selected tab index 
            if (selectedIndex < 0 || selectedIndex >= LocationTabView.Items.Count) // Validate the index is in range
            {
                return false;
            }

            var tabItem = LocationTabView.Items[selectedIndex] as SfTabItem; // Get the tab item at the selected index
            string headerText = tabItem?.Header! as string;             // Extract the tab header text (city name)
            if (string.IsNullOrWhiteSpace(headerText))                 // Ensure the header is a non-empty string
            {
                return false;
            }

            return CityCoordinates.TryGetValue(headerText, out coordinates); //  coordinates for the city name
        }

        private async Task LoadWeatherDataAsync((double Latitude, double Longitude) coordinates, CancellationToken cancellationToken) // Fetches and binds weather
        {
            // Build the Open‑Meteo API URL requesting hourly temperature + weather and daily max/min + weather
            string apiUrl =
                $"https://api.open-meteo.com/v1/forecast" +
                $"?latitude={coordinates.Latitude}&longitude={coordinates.Longitude}" +
                $"&hourly=temperature_2m,weathercode" +
                $"&daily=temperature_2m_max,temperature_2m_min,weathercode" +
                $"&timezone=auto";

            string jsonText = await SharedHttpClient.GetStringAsync(apiUrl, cancellationToken); // Download the JSON response as text
            var forecast = JsonSerializer.Deserialize<WeatherApiResponse>(jsonText);            // Deserialize JSON to typed objects

            if (forecast is null || forecast.Hourly is null || forecast.Daily is null)         // Ensure required sections exist
            {
                return;
            }

            // Determine current temperature from the first hourly value; if missing -> NaN
            double currentTemperature = (forecast.Hourly.TemperatureData?.Count ?? 0) > 0
                ? forecast.Hourly.TemperatureData![0]
                : double.NaN;

            // Determine current weather value from the first hourly entry; if missing, use -1 as an unknown value
            int currentWeather = (forecast.Hourly.Weather?.Count ?? 0) > 0
                ? forecast.Hourly.Weather![0]
                : -1;

            var mapping = MapWeatherCondition(currentWeather);         // Convert the numeric weather value to an icon and text label
            string currentIcon = mapping.Icon;                         // Extract the icon path
            string currentLabel = mapping.Label;                       // Extract the label

            TempLabel.Text = double.IsNaN(currentTemperature) ? "--" : $"{currentTemperature:0}°C"; // Show temperature or placeholder
            ConditionLabel.Text = currentLabel;                        // Show condition text (e.g., Clear, Rain)
            WeatherIcon.Source = currentIcon;                          // Show the corresponding icon

            BuildDailyBlocks(forecast);                                // Populate the upcoming days forecast section
        }

        private void BuildDailyBlocks(WeatherApiResponse forecast) // UI blocks for daily forecasts based on the WeatherApiResponse
        {
            NextDaysLayout.Children.Clear(); // Clear any previous child views from the layout that displays next days

            var daily = forecast.Daily; // reference to the daily data portion of the forecast

            // Compute how many days we can safely render by taking the minimum length
            // across all required lists to avoid index-out-of-range errors
            int dayCount = Math.Min(
                Math.Min(daily!.Time?.Count ?? 0, daily.TemperatureMax?.Count ?? 0),
                Math.Min(daily.TemperatureMin?.Count ?? 0, daily.WeatherData?.Count ?? 0)
            );

            if (dayCount <= 0) // If there are no days to render, exit early
            {
                return;
            }

            int startIndex = GetFirstFutureDayIndex(daily.Time!); // Determine the first index that represents a future day (skip today)

            if (startIndex < 0 || startIndex >= dayCount) // If the start index is invalid or out of range, exit early
            {
                return;
            }

            // Loop over each day from the first future day to the end of available data
            for (int dayIndex = startIndex; dayIndex < dayCount; dayIndex++)
            {
                DateTime? date = TryParseDate(daily.Time![dayIndex]);  // Parse the date string for the current day into a DateTime (nullable)

                // Format the date as "Wed • Jan 15"; fall back to weekday abbreviation if parsing fails
                string weekdayAndDate = date.HasValue
                    ? $"{date.Value:ddd} • {date.Value:MMM dd}"
                    : GetWeekdayAbbreviation(daily.Time[dayIndex]);

                int dailyWeather = daily.WeatherData![dayIndex];  // Get the numeric weather code for the day (e.g., Open-Meteo weathercode)

                var mapping = MapWeatherCondition(dailyWeather);  // Map the numeric weather code to UI-friendly data (e.g., icon path)

                string dailyIcon = mapping.Icon;  // Extract the icon path/name to display for this weather condition

                var block = new VerticalStackLayout   // To hold the day's icon and labels
                {
                    Spacing = 6,
                    HorizontalOptions = LayoutOptions.Center,
                    WidthRequest = 86
                };

                block.Children.Add(new Image  // For showing the weather icon
                {
                    Source = dailyIcon,
                    HeightRequest = 30,
                    HorizontalOptions = LayoutOptions.Center
                });

                block.Children.Add(new Label  // For showing the formatted weekday + date
                {
                    Text = weekdayAndDate,                  // e.g., "Wed • Jan 15"
                    TextColor = Colors.White,
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center
                });

                block.Children.Add(new Label  // Add a label for the high temperature (rounded to integer)
                {
                    Text = $"High : {daily.TemperatureMax![dayIndex]:0}°", // format without decimals
                    TextColor = Colors.White,
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Center
                });


                block.Children.Add(new Label   // Add a label for the low temperature (rounded to integer)
                {
                    Text = $"Low : {daily.TemperatureMin![dayIndex]:0}°",
                    TextColor = Colors.White,
                    FontSize = 16,
                    Opacity = 0.9,
                    HorizontalOptions = LayoutOptions.Center
                });

                NextDaysLayout.Children.Add(block);
            }
        }

        // Finds the index of the first date strictly after today; returns -1 on failure
        private static int GetFirstFutureDayIndex(List<string> dates)
        {
            //if list is null or empty, indicate failure
            if (dates == null || dates.Count == 0)
            {
                return -1;
            }

            // Use local device date; Open‑Meteo with timezone=auto aligns to device tz
            DateTime todayLocal = DateTime.Now.Date;

            for (int i = 0; i < dates.Count; i++)
            {
                if (DateTime.TryParse(dates[i], out var d))   // parse each date string into a DateTime
                {
                    if (d.Date > todayLocal)  // If the parsed date is strictly in the future, return its index
                    {
                        return i; // first future day
                    }
                }
            }

            // if parsing fails, mimic prior behavior by skipping index 0
            // Return 1 if there are at least 2 dates; otherwise, indicate failure
            return dates.Count > 1 ? 1 : -1;
        }

        private static DateTime? TryParseDate(string dateString)
        {
            return DateTime.TryParse(dateString, out var d) ? d : null; // convert a date string into a nullable DateTime
        }

        private static string GetWeekdayAbbreviation(string dateString) // Converts a date string to a weekday abbreviation
        {
            if (DateTime.TryParse(dateString, out var fullDate))       // Try parsing full date-time strings
            {
                return fullDate.ToString("ddd");                       // Return abbreviated weekday (e.g., Mon)
            }

            return dateString;                                         // If parsing fails, return the original string
        }

        private static (string Icon, string Label) MapWeatherCondition(int weatherValue) // Maps numeric weather value to icon and label
        {
            // Mapping based on Open‑Meteo’s documented weather values
            return weatherValue switch
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

    // Root response object for the Open‑Meteo forecast
    public class WeatherApiResponse
    {
        [JsonPropertyName("hourly")]                   // Maps the JSON "hourly" object to this property
        public HourlyData? Hourly { get; set; }         // Holds arrays for hourly temperature and weather values

        [JsonPropertyName("daily")]
        public DailyData? Daily { get; set; }           // Holds arrays for daily highs, lows, dates, and weather values
    }

    // Represents the hourly forecast data
    public class HourlyData
    {
        [JsonPropertyName("temperature_2m")]
        public List<double>? TemperatureData { get; set; } // Hourly temperatures

        [JsonPropertyName("weathercode")]
        public List<int>? Weather { get; set; }         // Hourly numeric weather values (used to choose icon/label)
    }

    // Represents the daily forecast data
    public class DailyData
    {
        [JsonPropertyName("time")]
        public List<string>? Time { get; set; }         // ISO date strings for each day

        [JsonPropertyName("temperature_2m_max")]
        public List<double>? TemperatureMax { get; set; } // high temperatures

        [JsonPropertyName("temperature_2m_min")]
        public List<double>? TemperatureMin { get; set; } // low temperatures

        [JsonPropertyName("weathercode")]
        public List<int>? WeatherData { get; set; }     // Daily numeric weather values (used to choose icon/label)
    }
}
