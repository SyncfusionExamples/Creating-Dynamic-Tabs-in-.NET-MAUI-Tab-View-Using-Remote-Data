using System.Text.Json.Serialization;

namespace DynamicTabs
{
    public class HourlyData
    {
        [JsonPropertyName("temperature_2m")]
        public List<double>? TemperatureData { get; set; } // Hourly temperature array

        [JsonPropertyName("weathercode")]
        public List<int>? Weather { get; set; } // Hourly weather code array
    }
}
