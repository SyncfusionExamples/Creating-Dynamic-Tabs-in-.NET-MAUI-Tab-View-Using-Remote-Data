using System.Text.Json.Serialization;

namespace DynamicTabs
{
    public class DailyData
    {
        [JsonPropertyName("time")]
        public List<string>? Time { get; set; } // Date strings for each day

        [JsonPropertyName("temperature_2m_max")]
        public List<double>? TemperatureMax { get; set; } // Daily max temperatures

        [JsonPropertyName("temperature_2m_min")]
        public List<double>? TemperatureMin { get; set; } // Daily min temperatures

        [JsonPropertyName("weathercode")]
        public List<int>? WeatherData { get; set; } // Daily weather codes
    }
}
