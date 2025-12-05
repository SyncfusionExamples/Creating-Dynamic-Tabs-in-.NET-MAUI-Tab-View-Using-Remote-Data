namespace DynamicTabs
{
    public partial class MainPage : ContentPage 
    {
        private readonly WeatherMainViewModel viewmodel = new(); // Create the main ViewModel instance

        public MainPage()
        {
            InitializeComponent(); 
            BindingContext = viewmodel; // Bind the page to the ViewModel

            // Hook tab selection change - load data for selected city in background
            // Do not await here; if needed it loads in background
            LocationTabView.SelectionChanged += (_, e) => // Event handler for TabView selection changes
            {
                int index = (int)e.NewIndex; // Get new selected tab index
                if (index >= 0 && index < viewmodel.Cities.Count) // Ensure index is valid
                    _ = viewmodel.Cities[index].EnsureLoadedAsync(); // Fire-and-forget loading of city data
            };
        }

        protected override async void OnAppearing() // Called when page appears on screen
        {
            base.OnAppearing(); // Call base implementation

            if (viewmodel.Cities.Count > 0) // If there are cities configured
            {
                LocationTabView.SelectedIndex = 0; // Select the first tab by default
                                                   // Load first tab, then prefetch others
                await viewmodel.WarmUpAsync(); // Load visible city first; prefetch others in background
            }
        }
    }
}
