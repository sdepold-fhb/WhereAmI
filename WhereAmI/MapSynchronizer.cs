using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace WhereAmI
{
    public class MapSynchronizer
    {
        Object[] maps;
        double lat, lon;
        int zoom;
        

        public MapSynchronizer(double lat, double lon, int zoom) {
            this.lat = lat;
            this.lon = lon;
            this.zoom = zoom;
        }

        public void updateLocation(double lat, double lon, int zoom)
        {
            /*double height = imageGoogleMaps.Height;
            double width = imageGoogleMaps.Width;

            bingMap.SetView(new System.Device.Location.GeoCoordinate(lat, lon), zoom);

            Uri googleMapsUri = new Uri("http://maps.google.com/maps/api/staticmap?center=" + lat + "," + lon + "&zoom=" + zoom + "&size=" + width + "x" + height + "&sensor=true", UriKind.Absolute);
            BitmapImage googleMapsSource = new BitmapImage(googleMapsUri);
            imageGoogleMaps.Source = googleMapsSource;

            Uri osmUri = new Uri("http://tah.openstreetmap.org/MapOf/?lat=" + lat + "&long=" + lon + "&z=" + zoom + "&w=" + width + "&h=" + height + "&format=png", UriKind.Absolute);
            BitmapImage osmSource = new BitmapImage(osmUri);
            imageOSM.Source = osmSource;*/
        }

    }
}
