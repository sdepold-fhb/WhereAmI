using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Media.Imaging;
using System.Device.Location;
using LocationServiceSample;
using Microsoft.Phone.Controls.Maps;

namespace WhereAmI
{
    public partial class MainPage : PhoneApplicationPage
    {
        IGeoPositionWatcher<GeoCoordinate> watcher;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
                App.ViewModel.LoadData();

            setMapLocations(52.515, 13.3331, 12);
            initWatcher();
        }

        private void initWatcher()
        {
            if (watcher == null)
            {
                GeoCoordinateEventMock[] events = new GeoCoordinateEventMock[] {
                    new  GeoCoordinateEventMock { Latitude=52.515, Longitude=13.3331, Time=new TimeSpan(0,0,10) },
                    new  GeoCoordinateEventMock { Latitude=52.3987, Longitude=13.0684, Time=new TimeSpan(0,0,10) },
                    new  GeoCoordinateEventMock { Latitude=52.4145, Longitude=12.5555, Time=new TimeSpan(0,0,10) }
                };

                watcher = new EventListGeoLocationMock(events);
                //watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcherPositionChanged);
                watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcherPositionChanged);
            }
            //watcher = new GeoCoordinateWatcher(accuracy);
            //watcher.MovementThreshold = 20;

            //watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
            //watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);

            watcher.Start(); 
        }

        private void watcherPositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => {
                setMapLocations(e.Position.Location.Latitude, e.Position.Location.Longitude, 12);
            });
        }

        private void setMapLocations(double lat, double lon, int zoom)
        {
            double height = imageGoogleMaps.Height;
            double width = imageGoogleMaps.Width;

            bingMap.SetView(new System.Device.Location.GeoCoordinate(lat, lon), zoom);

            Uri googleMapsUri = new Uri("http://maps.google.com/maps/api/staticmap?center=" + lat + "," + lon + "&zoom=" + zoom + "&size=" + width + "x" + height + "&sensor=true", UriKind.Absolute);
            BitmapImage googleMapsSource = new BitmapImage(googleMapsUri);
            imageGoogleMaps.Source = googleMapsSource;

            Uri osmUri = new Uri("http://tah.openstreetmap.org/MapOf/?lat=" + lat + "&long=" + lon + "&z=" + zoom + "&w=" + width + "&h=" + height + "&format=png", UriKind.Absolute);
            BitmapImage osmSource = new BitmapImage(osmUri);
            imageOSM.Source = osmSource;
        }
    }
}