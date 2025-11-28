using Syncfusion.Maui.TabView;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DynamicTabs
{
    public partial class MainPage : ContentPage
    {
        private WeatherPageController Controller { get; }
        public MainPage()
        {
            InitializeComponent();

            Controller = new WeatherPageController(
            dateLabel: DateLabel,
            tempLabel: TempLabel,
            conditionLabel: ConditionLabel,
            weatherIcon: WeatherIcon,
            nextDaysLayout: NextDaysLayout,
            locationTabView: LocationTabView,
            displayAlertAsync: (title, message, cancel) => DisplayAlertAsync(title, message, cancel));
        }
    }
}
