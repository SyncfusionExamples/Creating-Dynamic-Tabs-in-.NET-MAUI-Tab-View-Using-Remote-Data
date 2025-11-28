using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DynamicTabs
{
    public class HourlyData
    {
        [JsonPropertyName("temperature_2m")]
        public List<double>? TemperatureData { get; set; } // Hourly temperatures

        [JsonPropertyName("weathercode")]
        public List<int>? Weather { get; set; }         // Hourly numeric weather values (used to choose icon/label)
    }
}
