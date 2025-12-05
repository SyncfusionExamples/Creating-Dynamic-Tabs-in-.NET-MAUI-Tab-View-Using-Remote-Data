namespace DynamicTabs
{
    // Model for a single day's forecast item in the UI list
    public class DailyForecastItem
    {
        public string DayText { get; set; } = ""; // e.g., "Tue • Jan 30"
        public string HighText { get; set; } = ""; // e.g., "High : 25°"
        public string LowText { get; set; } = ""; // e.g., "Low : 12°"
        public string Icon { get; set; } = "cloudy.png"; // Icon file name
    }
}
