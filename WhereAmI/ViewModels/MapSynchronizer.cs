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

        // a list of http clients for loading images asynchronously
        List<WebClient> clients;

        // constructor of the MapSynchronizer
        // allows updates of maps and images
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

        // returns the dimension of the first added map or image
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

        // generates an URI for a static map according to the passed type
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

            // replace commas with points in lat and lon
            string url = String.Format(template, lat.ToString().Replace(',', '.'), lon.ToString().Replace(',', '.'), zoom, width, height);
            return new Uri(url, UriKind.Absolute);
        }

        // updates the view of maps and the URI of images
        private void updateMapLocations()
        {
            // only go on if there are actually maps or images
            if ((maps.Count == 0) && (images.Count == 0)) return;

            // get the dimension of the maps
            Dictionary<String, double> dimension = getMapDimension();

            // cancel all active image load tasks
            cancelImageLoadTasks();

            // update the view of all added maps
            foreach (KeyValuePair<String, Object> pair in maps)
                ((Map)pair.Value).SetView(new System.Device.Location.GeoCoordinate(lat, lon), zoom);

            // start the update process for each added image
            foreach (KeyValuePair<String, Object> pair in images)
                updateImage((Image)pair.Value, getMapUri(pair.Key, dimension["width"], dimension["height"]));

            // fire the MapsUpdated event
            EventHandler handler = MapsUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        // will cancel all asynchronous http requests
        private void cancelImageLoadTasks()
        {
            foreach (WebClient client in clients)
                client.CancelAsync();

            clients.Clear();
        }

        // creates a http client for loading the image asynchronously and adding it to the existing image when finished
        private void updateImage(Image i, Uri uri)
        {
            WebClient client = new WebClient();

            // update the source of the image when the read process is completed
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
