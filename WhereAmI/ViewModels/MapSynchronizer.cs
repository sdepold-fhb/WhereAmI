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
using System.Collections.Generic;
using Microsoft.Phone.Controls.Maps;
using System.Linq;
using System.Windows.Media.Imaging;

namespace WhereAmI
{
    public class MapSynchronizer
    {
        double lat, lon;
        int zoom;
        public Dictionary<String, Object> maps, images;
        public event EventHandler MapsUpdated;

        public MapSynchronizer(double lat, double lon, int zoom) {  
            this.lat = lat;
            this.lon = lon;
            this.zoom = zoom;
            maps = new Dictionary<string, object>();
            images = new Dictionary<string, object>();
        }

        public int getZoom()
        {
            return zoom;
        }
        
        public void update(int zoom)
        {
            update(lat, lon, zoom);
        }

        public void update(double lat, double lon)
        {
            update(lat, lon, zoom);
        }

        public void update(double lat, double lon, int zoom)
        {
            this.lat = lat;
            this.lon = lon;
            this.zoom = zoom;
            updateMapLocations();
        }

        private Dictionary<String, double> getMapDimension()
        {
            Dictionary<String, double> result = new Dictionary<string,double>();

            result.Add("width", 0);
            result.Add("height", 0);

            if (maps.Count != 0)
            {
                Map map = (Map)maps.FirstOrDefault().Value;
                result["height"] = map.Height;
                result["width"] = map.Width;
            } else if (images.Count != 0) {
                Image image = (Image)images.FirstOrDefault().Value;
                result["height"] = image.Height;
                result["width"] = image.Width;
            }

            return result;
        }

        private Uri getMapUri(string type, double width, double height)
        {
            string result = "";

            switch (type)
            {
                case "google":
                    result = "http://maps.google.com/maps/api/staticmap?center=" + lat + "," + lon + "&zoom=" + zoom + "&size=" + width + "x" + height + "&sensor=true";
                    break;
                case "osm":
                    result = "http://tah.openstreetmap.org/MapOf/?lat=" + lat + "&long=" + lon + "&z=" + zoom + "&w=" + width + "&h=" + height + "&format=png";
                    break;
            }

            return new Uri(result, UriKind.Absolute);
        }

        private void updateMapLocations()
        {
            if ((maps.Count == 0) && (images.Count == 0)) return;

            Dictionary<String, double> dimension = getMapDimension();

            foreach (KeyValuePair<String, Object> pair in maps)
                ((Map)pair.Value).SetView(new System.Device.Location.GeoCoordinate(lat, lon), zoom);

            foreach (KeyValuePair<String, Object> pair in images)
                ((Image)pair.Value).Source = new BitmapImage(getMapUri(pair.Key, dimension["width"], dimension["height"]));

            EventHandler handler = MapsUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
