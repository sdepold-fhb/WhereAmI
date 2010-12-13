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
using System.Collections;
using System.Collections.Generic;
using Microsoft.Phone.Controls.Maps;
using System.Linq;
using System.Windows.Media.Imaging;
using System.IO;

namespace WhereAmI
{
    public class MapSynchronizer
    {
        public Dictionary<String, Object> maps, images;
        public event EventHandler MapsUpdated;
        const string BING_KEY = "AslbzCOX3iwxX97TSAf28_rxjy-Z6HrIQZhAh6wgB18kBK7LOOGzTFCiFNHN-Ruk";

        double lat, lon;
        int zoom;
        bool showAlternative;

        List<WebClient> clients;

        public MapSynchronizer() {
            lat = lon = 0;
            zoom = 12;
            showAlternative = false;

            maps = new Dictionary<string, object>();
            images = new Dictionary<string, object>();

            clients = new List<WebClient>();
        }

        public int getZoom()
        {
            return zoom;
        }

        public void update(bool showAlternative)
        {
            update(lat, lon, zoom, showAlternative);
        }

        public void update(int zoom)
        {
            update(lat, lon, zoom, showAlternative);
        }

        public void update(double lat, double lon)
        {
            update(lat, lon, zoom, showAlternative);
        }

        public void update(double lat, double lon, int zoom, bool showAlternative)
        {
            this.lat = lat;
            this.lon = lon;
            this.zoom = zoom;
            this.showAlternative = showAlternative;
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
            string template = "";

            switch (type)
            {
                case "google":
                    template = "http://maps.google.com/maps/api/staticmap?center={0},{1}&zoom={2}&size={3}x{4}&sensor=true&markers=color:blue|{0},{1}";
                    if (showAlternative) template += "&maptype=satellite";
                    break;
                case "osm":
                    template = "http://staticmap.openstreetmap.de/staticmap.php?center={0},{1}&zoom={2}&size={3}x{4}&markers={0},{1},red-pushpin";
                    if (showAlternative) template += "&maptype=cycle";
                    break;
                case "bing":
                    template = "http://dev.virtualearth.net/REST/v1/Imagery/Map/Road/{0},{1}/{2}?mapSize={3},{4}&pushpin={0},{1};35&key=" + BING_KEY;
                    if (showAlternative) template = template.Replace("/Road/", "/Aerial/");
                    break;
            }

            return new Uri(String.Format(template, lat, lon, zoom, width, height), UriKind.Absolute);
        }

        private void updateMapLocations()
        {
            if ((maps.Count == 0) && (images.Count == 0)) return;

            Dictionary<String, double> dimension = getMapDimension();
            cancelImageLoadTasks();

            foreach (KeyValuePair<String, Object> pair in maps)
                ((Map)pair.Value).SetView(new System.Device.Location.GeoCoordinate(lat, lon), zoom);

            foreach (KeyValuePair<String, Object> pair in images)
                updateImage((Image)pair.Value, getMapUri(pair.Key, dimension["width"], dimension["height"]));

            EventHandler handler = MapsUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void cancelImageLoadTasks()
        {
            foreach (WebClient client in clients)
                client.CancelAsync();

            clients.Clear();
        }

        private void updateImage(Image i, Uri uri)
        {
            WebClient client = new WebClient();

            client.OpenReadCompleted += new OpenReadCompletedEventHandler(delegate(object sender, OpenReadCompletedEventArgs e) {
                try
                {
                    BitmapImage imageToLoad = new BitmapImage();
                    imageToLoad.SetSource(e.Result as Stream);
                    i.Source = imageToLoad;
                } catch (Exception) {
                }
            });

            client.OpenReadAsync(uri, uri.AbsoluteUri);
            clients.Add(client);
        }
    }
}
