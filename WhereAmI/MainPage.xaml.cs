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
using Microsoft.Devices;

namespace WhereAmI
{
    public partial class MainPage : PhoneApplicationPage
    {
        GeoCoordinateWatcher watcher;
        IGeoPositionWatcher<GeoCoordinate> watcherMock;
        MapSynchronizer ms;

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
            // MapSynchronize updates added maps
            ms = new MapSynchronizer();

            // add bing, google and osm
            ms.images.Add("bing", imageBingMaps);
            ms.images.Add("google", imageGoogleMaps);
            ms.images.Add("osm", imageOSM);

            // update the zoom-slider-label after maps have been updated
            ms.MapsUpdated += setZoomSliderLabel;

            // initialize the MapSynchronizer
            ms.update(52.515, 13.3331, 12, false);

            // use a mock for the emulator and real watcher else
            if (Microsoft.Devices.Environment.DeviceType == DeviceType.Emulator)
                initGeoLocationMock();
            else
                initGeoLocationWatcher();

            // init zoom slider
            initZoomSlider();
        }

        private void initGeoLocationMock()
        {
            if (watcherMock == null)
            {
                GeoCoordinateEventMock[] events = new GeoCoordinateEventMock[] {
                    new  GeoCoordinateEventMock { Latitude=52.515, Longitude=13.3331, Time=new TimeSpan(0,0,20) },
                    new  GeoCoordinateEventMock { Latitude=52.3987, Longitude=13.0684, Time=new TimeSpan(0,0,20) },
                    new  GeoCoordinateEventMock { Latitude=52.4145, Longitude=12.5555, Time=new TimeSpan(0,0,20) }
                };

                watcherMock = new EventListGeoLocationMock(events);
                watcherMock.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcherPositionChanged);
            }

            watcherMock.Start(); 
        }

        private void initGeoLocationWatcher()
        {
            if (watcher == null)
            {
                watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
                watcher.MovementThreshold = 20;
                watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcherPositionChanged);
            }
            
            watcher.Start();
        }

        // update the MapSynchronizer after changes of the geolocation
        private void watcherPositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => {
                ms.update(e.Position.Location.Latitude, e.Position.Location.Longitude);
            });
        }

        // observe zoomslider changes
        private void initZoomSlider()
        {
            zoomSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(zoomSliderValueChanged);
        }

        // sets the label of the zoomslider
        private void setZoomSliderLabel(object sender, EventArgs args)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => {
                zoomText.Text = "Zoom-Level: " + ms.getZoom();
            });
        }

        // updates the MapSynchronizer after changes of the zoomslider
        private void zoomSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => {
                ms.update((int) e.NewValue);
            });
        }

        // updates the MapSynchronizer when alternative mode was enabled
        private void showAlternativeModeCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            ms.update(true);
        }

        // updates the MapSynchronizer when alternative mode was disabled
        private void showAlternativeModeCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            ms.update(false);
        }
    }
}