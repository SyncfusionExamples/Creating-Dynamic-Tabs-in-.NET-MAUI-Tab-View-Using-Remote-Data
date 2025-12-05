using System.Text.Json.Serialization;

namespace DynamicTabs
{
    public class WeatherApiResponse
    {
        [JsonPropertyName("hourly")]
        public HourlyData? Hourly { get; set; } // Hourly data node

        [JsonPropertyName("daily")]
        public DailyData? Daily { get; set; } // Daily data node
    }
}
