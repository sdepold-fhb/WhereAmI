/* Mock Geo location class library to simulate GeoCoordinate information in the Windows
 * Phone developer tools CTP.
 * 
 * Sample library provided as-is without warranty and should not be used in production applications
 * THIS IS A DEBUG TOOL ONLY
 * 
 * Use:
 * 
     * IGeoPostitionWatcher<GeoCoordinate> watcher;
     * GeoCoordinateEventMock[] events = new GeoCoordinateEventMock[] {
     *      new  GeoCoordinateEventMock { Latitude=34.4, Longitude=11.2, Time=new TimeSpan(0,0,5) },
     *      new  GeoCoordinateEventMock { Latitude=31.4, Longitude=21.2, Time=new TimeSpan(0,0,1) },
     *      new  GeoCoordinateEventMock { Latitude=34.3, Longitude=28.2, Time=new TimeSpan(0,0,2) },
     *      new  GeoCoordinateEventMock { Latitude=32.4, Longitude=34.2, Time=new TimeSpan(0,0,3) },
     *      new  GeoCoordinateEventMock { Latitude=31.2, Longitude=37.2, Time=new TimeSpan(0,0,4) },
     *      new  GeoCoordinateEventMock { Latitude=33.73, Longitude=39.2, Time=new TimeSpan(0,0,5) },
     *      new  GeoCoordinateEventMock { Latitude=31.87, Longitude=41.2, Time=new TimeSpan(0,0,6) },
     *      new  GeoCoordinateEventMock { Latitude=11.81, Longitude=42.2, Time=new TimeSpan(0,0,7) }
     *      };
     *      
     * watcher = new EventListGeoLocationMock(events);
     * watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
     * watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
     * watcher.Start();
 * 
 * */
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Threading;

namespace LocationServiceSample
{
    abstract public class GeoLocationMock : IGeoPositionWatcher<GeoCoordinate>
    {
        #region Properties
        GeoPosition<GeoCoordinate> _current;
        Timer _timer;
        #endregion

        #region Constructors
        public GeoLocationMock()
        {
            _current = new GeoPosition<GeoCoordinate>();
            Status = GeoPositionStatus.Initializing;
            RaiseStatusChanged();
        }
        #endregion

        #region Methods
        private void RaiseStatusChanged()
        {
            GeoPositionStatusChangedEventArgs args = new GeoPositionStatusChangedEventArgs(this.Status);
            if (StatusChanged != null)
            {
                StatusChanged(this, args);
            }
        }

        private void RaisePositionChanged()
        {
            GeoPositionChangedEventArgs<GeoCoordinate> args = new GeoPositionChangedEventArgs<GeoCoordinate>(_current);
            if (PositionChanged != null)
            {
                PositionChanged(this, args);
            }
        }

        public void OnTimerCallback(object state)
        {
            try
            {
                if (Status == GeoPositionStatus.Initializing)
                {
                    Status = GeoPositionStatus.NoData;
                    RaiseStatusChanged();
                }
                StartGetCurrentPosition();
                TimeSpan next = GetNextInterval();
                _timer.Change(next, next);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        protected void UpdateLocation(double longitude, double latitude)
        {
            GeoCoordinate location = new GeoCoordinate(latitude, longitude);
            if (!location.Equals(_current.Location))
            {
                _current = new GeoPosition<GeoCoordinate>(DateTimeOffset.Now, location);
                if (Status != GeoPositionStatus.Ready)
                {
                    Status = GeoPositionStatus.Ready;
                    RaiseStatusChanged();
                }
                RaisePositionChanged();
            }
        }

        abstract protected TimeSpan GetNextInterval();
        abstract protected void StartGetCurrentPosition();
        #endregion

        #region IGeoPositionWatcher<GeoCoordinate> Members
        public GeoPositionPermission Permission
        {
            get { return GeoPositionPermission.Granted; }
        }

        public GeoPosition<GeoCoordinate> Position
        {
            get { return _current; }
        }

        public event EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>> PositionChanged;

        public void Start(bool suppressPermissionPrompt)
        {
            Start();
        }

        public void Start()
        {
            TimeSpan span = GetNextInterval();
            _timer = new Timer(OnTimerCallback, null, span, span);
        }

        public GeoPositionStatus Status
        {
            get;
            protected set;
        }

        public event EventHandler<GeoPositionStatusChangedEventArgs> StatusChanged;

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            Status = GeoPositionStatus.Disabled;
            RaiseStatusChanged();
        }

        public bool TryStart(bool suppressPermissionPrompt, TimeSpan timeout)
        {
            Start();
            return true;
        }

        #endregion
    }

    public class GeoCoordinateEventMock
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public TimeSpan Time { get; set; } /// The time since previous event 
    }

    public class EventListGeoLocationMock : GeoLocationMock
    {
        List<GeoCoordinateEventMock> events;
        int currentEventId;

        public EventListGeoLocationMock(IEnumerable<GeoCoordinateEventMock> events)
        {
            this.events = new List<GeoCoordinateEventMock>(events);
            this.currentEventId = 0;
        }

        private GeoCoordinateEventMock Current
        {
            get
            {
                return events[currentEventId % events.Count];
            }
        }

        protected override void StartGetCurrentPosition()
        {
            this.UpdateLocation(Current.Longitude, Current.Latitude);
            currentEventId++;
        }

        protected override TimeSpan GetNextInterval()
        {
            return Current.Time;
        }
    }
}