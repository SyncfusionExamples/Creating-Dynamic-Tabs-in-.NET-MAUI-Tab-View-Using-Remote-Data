using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicTabs
{
    // Maps Open-Meteo weather codes to icon file names and display labels
    public static class WeatherMapping
    {
        public static (string Icon, string Label) Map(int weatherValue) // Returns tuple of icon and label
            => weatherValue switch // Pattern matching on weather code
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
                _ => ("cloudy.png", "Unknown") // Default for unmapped codes
            };
    }

}
