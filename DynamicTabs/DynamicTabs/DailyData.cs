using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DynamicTabs
{
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
