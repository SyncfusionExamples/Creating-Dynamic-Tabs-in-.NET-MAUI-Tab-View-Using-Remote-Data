using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DynamicTabs
{
    public class WeatherApiResponse
    {
        [JsonPropertyName("hourly")]                   // Maps the JSON "hourly" object to this property
        public HourlyData? Hourly { get; set; }         // Holds arrays for hourly temperature and weather values

        [JsonPropertyName("daily")]
        public DailyData? Daily { get; set; }           // Holds arrays for daily highs, lows, dates, and weather values
    }
}
