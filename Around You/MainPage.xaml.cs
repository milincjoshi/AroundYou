using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Around_You.Resources;
using System.Device.Location;
using Windows.Devices.Geolocation;
using System.Net.Http;
using Newtonsoft.Json;
using System.Windows.Media.Imaging;

namespace Around_You
{

    
   
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            Loaded += MainPage_Loaded;


            // Sample code to localize the ApplicationBar
            BuildLocalizedApplicationBar();
        }

        private static void SetProgressIndicator(bool isVisible) {
                SystemTray.ProgressIndicator.IsIndeterminate = isVisible;
            SystemTray.ProgressIndicator.IsVisible = isVisible;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();

            UpdateMap();
        }

        private async void UpdateMap() {

            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50 ;

            SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = "Getting GPS Location";
            try
            {
                Geoposition geoposition = await geolocator.GetGeopositionAsync(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(30));

                SystemTray.ProgressIndicator.Text = "Acquired";

                GeoCoordinate geocoordinate_center = new GeoCoordinate();
                geocoordinate_center.Latitude = geoposition.Coordinate.Latitude;
                geocoordinate_center.Longitude = geoposition.Coordinate.Longitude;

                //SetView(GeoCoordinate);
                //GeoCoordinate((lat,loong),Zoom Level)
                //Aroundyoumap.SetView(new GeoCoordinate(22.720132, 71.649536), 17D);


                // Aroundyoumap.SetView(geocoordinate_center, 17D);

                // ResultTextBlock.Text = string.Format("{0}-{1}",Aroundyoumap.Center.Latitude, Aroundyoumap.Center.Longitude);

                Aroundyoumap.Center = geocoordinate_center;
                Aroundyoumap.ZoomLevel = 15;

                SetProgressIndicator(false);

            }
            catch(UnauthorizedAccessException)
            {
                    MessageBox.Show("Location is disabled in Phone settings");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
    
            

        }



        // Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/feature.search.png", UriKind.Relative));
            appBarButton.Text = "Search";
            appBarButton.Click += appBarButton_Click;
            ApplicationBar.Buttons.Add(appBarButton);

            // Create a new menu item with the localized string from AppResources.
           // ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
           // ApplicationBar.MenuItems.Add(appBarMenuItem);
        }

        void appBarButton_Click(object sender, EventArgs e)
        {
            string topic = HttpUtility.UrlEncode(SearchTopic.Text);

            string navTo = string.Format("/Search_results.xaml?latitude={0}&longitude={1}&topic={2}&radius={3}",
                Aroundyoumap.Center.Latitude,
                Aroundyoumap.Center.Longitude,
                topic,5);

            NavigationService.Navigate(new Uri(navTo,UriKind.RelativeOrAbsolute));
        }
    }
}