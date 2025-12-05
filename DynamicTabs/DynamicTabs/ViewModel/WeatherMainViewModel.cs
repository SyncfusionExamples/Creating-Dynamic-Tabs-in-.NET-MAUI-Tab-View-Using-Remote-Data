using System.Collections.ObjectModel;

namespace DynamicTabs
{
    // Main ViewModel that manages multiple cities and warm-up logic
    public class WeatherMainViewModel
    {
        public ObservableCollection<CityWeatherViewModel> Cities { get; } = new(); // Observable list of city view models

        private readonly Dictionary<string, (double Lat, double Lon)> CityCoordinates = new() // City presets
    {
        { "Phoenix",        (33.45, -112.07) },
        { "Seattle",        (47.61, -122.33) },
        { "San Francisco",  (37.77, -122.42) },
        { "Miami",          (25.76, -80.19)  },
        { "Denver",         (39.74, -104.99) },
        { "Chicago",        (41.88, -87.63)  },
        { "New York",       (40.71, -74.01)  },
    };

        public WeatherMainViewModel()
        {
            foreach (var coordinates in CityCoordinates) // Create a city view model for each preset
            {
                Cities.Add(new CityWeatherViewModel(coordinates.Key, coordinates.Value.Lat, coordinates.Value.Lon));
            }
        }

        // Load the first city, then prefetch the rest
        public async Task WarmUpAsync()
        {
            if (Cities.Count == 0) return; // No cities to load

            await Cities[0].EnsureLoadedAsync(); // visible tab
                                                 // Prefetch others in background 
            _ = Task.WhenAll(Cities.Skip(1).Select(c => c.EnsureLoadedAsync())); // Fire-and-forget prefetch remaining cities
        }
    }
}
