using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Animation;
using Around_You.Resources;

namespace Around_You
{
    public partial class Search_results : PhoneApplicationPage
    {
        private double _latitude, _longitude;
        private double _radius;
        private string _topic;

        //apikey
        private const string flickrApiKey = "e424aad8bff9f275cdb417f0eebf3aad";

        public Search_results()
        {
            InitializeComponent();
            Loaded += Search_results_Loaded;
            BuildLocalizedapplicationBar();

        }
        private void BuildLocalizedapplicationBar() {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = true;

            ApplicationBarIconButton apib = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Check.png",UriKind.RelativeOrAbsolute));
            apib.Text = AppResources.AppBarSet;
            apib.Click+=apib_Click;
            ApplicationBar.Buttons.Add(apib);
        }

        private async void apib_Click(object sender, EventArgs e)
        {

            List<FlickrImage> imgs = new List<FlickrImage>();
            foreach (var item in PhotosForLockscreen.SelectedItems)
            {
                FlickrImage img = item as FlickrImage;
                if (img!=null)
                {
                    imgs.Add(img);
                }
            }

            LockScreenHelpers.CleanStorage();

            LockScreenHelpers.SaveSelectedBackgroundscreens(imgs);

            //calling this method for setting random image to lockscreen
            await LockScreenHelpers.SetRandomImageFromLocalstorage();

            MessageBox.Show("You have a new background.","Set",MessageBoxButton.OK);

        }

        protected async void Search_results_Loaded(object sender, RoutedEventArgs e)
        {
           // LocationTextblock.Text = string.Format("Location : {0} - {1} ",_latitude.ToString(), _longitude.ToString());
            OverlayProgressbar.Visibility = Visibility.Visible;
            OverlayProgressbar.IsIndeterminate = true;

            var images = await FlickrImage.GetFlickrImages(flickrApiKey,_topic,_latitude,_longitude,_radius);
            DataContext = images;

            if (images.Count == 0)
                NoPhotosfound.Visibility =  Visibility.Visible;
            else
                NoPhotosfound.Visibility = Visibility.Collapsed;

            OverlayProgressbar.Visibility = Visibility.Collapsed;
            OverlayProgressbar.IsIndeterminate =  false;

        
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _latitude = Convert.ToDouble(NavigationContext.QueryString["latitude"]);
            _longitude = Convert.ToDouble(NavigationContext.QueryString["longitude"]);
            _topic = NavigationContext.QueryString["topic"];
            _radius = Convert.ToDouble(NavigationContext.QueryString["radius"]);
 
        }

        private void PhotosForLockscreen_SelectonChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PhotosForLockscreen.SelectedItems.Count == 0)
                ApplicationBar.IsVisible = false;
            else
                ApplicationBar.IsVisible = true;
        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            if (img == null)
                return;
            
            //using animation
            Storyboard s = new Storyboard();

            DoubleAnimation doubleani = new DoubleAnimation();
            doubleani.To = 1;
            doubleani.Duration = new Duration(TimeSpan.FromMilliseconds(500));

            Storyboard.SetTarget( doubleani, img);
            Storyboard.SetTargetProperty(doubleani, new PropertyPath(OpacityProperty));

            s.Children.Add(doubleani);
            s.Begin();


        }
    }
}