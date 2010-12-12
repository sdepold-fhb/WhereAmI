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
            ms = new MapSynchronizer(1, 2, 3);
            ms.maps.Add("bing", bingMap);
            ms.images.Add("google", imageGoogleMaps);
            ms.images.Add("osm", imageOSM);
            ms.MapsUpdated += setZoomSliderLabel;
            ms.update(52.515, 13.3331, 12);

            initGeoLocationMock();
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
                //watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcherPositionChanged);
                watcherMock.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcherPositionChanged);
            }
            //watcher = new GeoCoordinateWatcher(accuracy);
            //watcher.MovementThreshold = 20;

            //watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
            //watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);

            watcherMock.Start(); 
        }

        private void initGeoLocation()
        {

        }

        private void watcherPositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => {
                ms.update(e.Position.Location.Latitude, e.Position.Location.Longitude);
            });
        }

        private void initZoomSlider()
        {
            zoomSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(zoomSliderValueChanged);
        }

        private void setZoomSliderLabel(object sender, EventArgs args)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => {
                zoomText.Text = "Zoom-Level: " + ms.getZoom();
            });
        }

        private void zoomSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => {
                ms.update((int) e.NewValue);
                zoomText.Text = "Zoom-Level: " + ms.getZoom();
            });
        }
    }
}