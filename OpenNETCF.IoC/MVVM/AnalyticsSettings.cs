using OpenNETCF.GA;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OpenNETCF.IoC
{
    public class AnalyticsSettings
    {
        private static AnalyticsService m_analytics;
        private bool m_inSession;
        private int m_lastPageShownTick;

        internal bool ScreenTrackingEnabled { get; private set; }
        internal bool TimingTrackingEnabled { get; private set; }

        public const string DefaultTimingCategory = "Navigation";

        public string TimingCategory { get; set; }

        internal AnalyticsSettings()
        {
            // did someone already register analytics? If so, we'll just use that
            var test = AnalyticsService;
            TimingCategory = DefaultTimingCategory;
        }

        private AnalyticsService AnalyticsService
        {
            get
            {
                if (m_analytics == null)
                {
                    var existing = RootWorkItem.Services.Get<AnalyticsService>();
                    if (existing != null)
                    {
                        m_analytics = existing;
                    }
                }

                return m_analytics;
            }
        }

        public void Register(string trackingID, string appName, Version appVersion = null, string appID = null)
        {
            m_analytics = new AnalyticsService(trackingID, applicationName: appName, applicationVersion: appVersion);
            RootWorkItem.Services.Add(m_analytics);
        }

        public void EnableScreenTracking()
        {
            if (m_analytics == null) throw new Exception("Analytics not initialized.");
            ScreenTrackingEnabled = true;
        }

        public void DisableScreenTracking()
        {
            if (m_analytics == null) throw new Exception("Analytics not initialized.");
            ScreenTrackingEnabled = false;
        }

        public void EnableTimingTracking()
        {
            if (m_analytics == null) throw new Exception("Analytics not initialized.");
            TimingTrackingEnabled = true;
        }

        public void DisableTimingTracking()
        {
            if (m_analytics == null) throw new Exception("Analytics not initialized.");
            TimingTrackingEnabled = false;
        }

        public void StartTrackingSession()
        {
            if (m_analytics == null) throw new Exception("Analytics not initialized.");

            m_analytics.TrackSessionStart();
            m_inSession = true;
        }

        public void StopTrackingSession()
        {
            if (m_analytics == null) throw new Exception("Analytics not initialized.");

            m_analytics.TrackSessionEnd();
            m_inSession = false;
        }

        public void TrackEvent(string category, string action, string label = null, string value = null)
        {
            if (m_analytics == null) throw new Exception("Analytics not initialized.");

            m_analytics.TrackEvent(category, action, label, value);
        }

        public void TrackException(Exception exception, bool isFatal = false)
        {
            TrackException(string.Format("{0}: {1}", exception.GetType().Name, exception.Message), isFatal);
        }

        public void TrackException(string description, bool isFatal = false)
        {
            if (m_analytics == null) throw new Exception("Analytics not initialized.");

            m_analytics.TrackException(description, isFatal);
        }

        public void TrackScreenView(string screenName)
        {
            if (m_analytics == null) throw new Exception("Analytics not initialized.");

            m_analytics.TrackScreenView(screenName);
        }

        public void AddTrackingAlias<TPage>(string trackAs)
            where TPage : Page
        {
            if (trackAs.IsNullOrEmpty()) return;

            var typeKey = typeof(TPage);
            var name = typeKey.Name;

            lock (m_screenAliasDictionary)
            {
                if (m_screenAliasDictionary.ContainsKey(typeKey))
                {
                    // replace
                    m_screenAliasDictionary[typeKey] = trackAs;
                }
                else
                {
                    m_screenAliasDictionary.Add(typeKey, trackAs);
                }
            }
        }

        public void TrackScreenView(Page page)
        {
            if (m_analytics == null) throw new Exception("Analytics not initialized.");

            var screenName = GetPageName(page);
            m_analytics.TrackScreenView(screenName);
        }

        private Dictionary<Type, string> m_screenAliasDictionary = new Dictionary<Type, string>();

        internal string GetPageName(Page page)
        {
            if (page == null) return null;

            var typeKey = page.GetType();
            var name = GetPageName(typeKey);

            if(name == null) name = page.Title;

            if (name.IsNullOrEmpty())
            {
                name = typeKey.Name;
            }

            return name;
        }

        private string GetPageName(Type pageType)
        {
            if (pageType == null) return null;

            lock (m_screenAliasDictionary)
            {
                if (m_screenAliasDictionary.ContainsKey(pageType))
                {
                    return m_screenAliasDictionary[pageType];
                }
            }

            return null;
        }

        internal void LogPageNavigation(string fromPage, string toPage)
        {
            if (!ScreenTrackingEnabled) return;

            if (m_analytics == null) throw new Exception("Analytics not initialized.");

            m_analytics.TrackScreenView(toPage);

            var now = Environment.TickCount;
            if (!fromPage.IsNullOrEmpty())
            {
                if (m_lastPageShownTick != 0)
                {
                    if (TimingTrackingEnabled)
                    {
                        var delta = now - m_lastPageShownTick;
                        AnalyticsService.TrackTiming(TimingCategory, fromPage, delta);
                    }
                }
            }
            m_lastPageShownTick = now;
        }
    }
}