using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;

using Xamarin.Forms;

namespace OpenNETCF.GA
{
    public class AnalyticsService
    {
        public string Version { get; private set; }
        public string TrackingID { get; private set; }
        public string UserAgent { get; set; }
        public string ClientID { get; set; }
        public bool AnonymizeIP { get; set; }
        public string DataSource { get; set; }
        public string ApplicationName { get; set; }
        public string ApplicationID { get; set; }
        public Version ApplicationVersion { get; set; }

        private const string BaseURL = "https://www.google-analytics.com/collect";

        private Dictionary<string, string> m_parameters;

        public AnalyticsService(string trackingID, string clientID = null, string applicationName = null, Version applicationVersion = null)
        {
            m_parameters = new Dictionary<string, string>();

            // set defaults
            Version = "1";
            AnonymizeIP = false;
            DataSource = "app";

            ApplicationName = applicationName;
            ApplicationVersion = applicationVersion;

            // TODO: validate these a little better
            if (string.IsNullOrEmpty(trackingID)) throw new ArgumentException("Invalid trackingID");
            TrackingID = trackingID;

            if (string.IsNullOrEmpty(clientID))
            {
                clientID = Guid.NewGuid().ToString();
            }

            ClientID = clientID;

            // populate a user agent based on current environment
            UserAgent = UserAgentGenerator.GetPlatformUserAgent();
        }

        public void TrackScreenView(string screenName, string appName = null, Version appVersion = null, string appID = null, string appInstallerID = null)
        {
            var plist = new List<TrackingParameter>();

            // TODO: do parameter sanity checks
            plist.Add(new TrackingParameter(ParameterName.ScreenName, screenName));

            // user can override the class-level value here
            if (!string.IsNullOrEmpty(appName))
            {
                plist.Add(new TrackingParameter(ParameterName.ApplicationName, appName));
            }
            else if (!string.IsNullOrEmpty(ApplicationName))
            {
                plist.Add(new TrackingParameter(ParameterName.ApplicationName, ApplicationName));
            }

            if (!string.IsNullOrEmpty(appID))
            {
                plist.Add(new TrackingParameter(ParameterName.ApplicationID, appID));
            }
            else if (!string.IsNullOrEmpty(ApplicationID))
            {
                plist.Add(new TrackingParameter(ParameterName.ApplicationID, ApplicationID));
            }

            if (appVersion != null)
            {
                plist.Add(new TrackingParameter(ParameterName.ApplicationVersion, appVersion.ToString(4)));
            }
            else if (ApplicationVersion != null)
            {
                plist.Add(new TrackingParameter(ParameterName.ApplicationVersion, ApplicationVersion.ToString(4)));
            }

            if (!string.IsNullOrEmpty(appID))
            {
                plist.Add(new TrackingParameter(ParameterName.ApplicationID, appID));
            }
            if (!string.IsNullOrEmpty(appInstallerID))
            {
                plist.Add(new TrackingParameter(ParameterName.ApplicationInstallerID, appInstallerID));
            }

            Track(HitType.ScreenView, plist);
        }

        public void TrackSessionStart()
        {
            var plist = new List<TrackingParameter>();
            plist.Add(new TrackingParameter(ParameterName.SessionControl, "start"));

            Track(HitType.Event, plist);
        }

        public void TrackSessionEnd()
        {
            var plist = new List<TrackingParameter>();
            plist.Add(new TrackingParameter(ParameterName.SessionControl, "end"));

            Track(HitType.Event, plist);
        }

        public void TrackEvent(string category, string action, string label = null, string value = null)
        {
            var plist = new List<TrackingParameter>();

            // TODO: do parameter sanity checks
            plist.Add(new TrackingParameter(ParameterName.EventCategory, category));
            plist.Add(new TrackingParameter(ParameterName.EventAction, action));

            if (!string.IsNullOrEmpty(label))
            {
                plist.Add(new TrackingParameter(ParameterName.EventLabel, label));
            }
            if (!string.IsNullOrEmpty(value))
            {
                plist.Add(new TrackingParameter(ParameterName.EventValue, value));
            }

            Track(HitType.Event, plist);
        }

        public void TrackTiming(string category, string variable, int timeInMilliseconds, string label = null)
        {
            var plist = new List<TrackingParameter>();

            // TODO: do parameter sanity checks
            plist.Add(new TrackingParameter(ParameterName.TimingCategory, category));
            plist.Add(new TrackingParameter(ParameterName.TimingVariable, variable));
            plist.Add(new TrackingParameter(ParameterName.TimingTime, timeInMilliseconds.ToString()));

            if (!string.IsNullOrEmpty(label))
            {
                plist.Add(new TrackingParameter(ParameterName.TimingLabel, label));
            }

            Track(HitType.Timing, plist);
        }

        private void Track(string hitType, List<TrackingParameter> parameters)
        {
            var param = new StringBuilder(1024);
            param.AppendFormat("v={0}&tid={1}&cid={2}&t={3}",
                Version,
                TrackingID,
                ClientID,
                hitType);

            if (AnonymizeIP)
            {
                parameters.Add(new TrackingParameter(ParameterName.AnonymizeIP, "1"));
            }

            if (!string.IsNullOrEmpty(DataSource))
            {
                parameters.Add(new TrackingParameter(ParameterName.DataSource, DataSource));
            }

            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    param.AppendFormat("&{0}={1}",
                        p.Key,
                        System.Net.WebUtility.UrlEncode(p.Value));
                }
            }

            SendTrackingData(param.ToString());
        }

        private async void SendTrackingData(string payload)
        {
            Debug.WriteLine("GA Tracking: " + payload);

            using (var client = new HttpClient())
            {

                try
                {
                    if(!string.IsNullOrEmpty(UserAgent))
                    {
                        client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgent);
                    }

                    var content = new StringContent(payload);
                    var result = await client.PostAsync(BaseURL, content);
                    if (!result.IsSuccessStatusCode)
                    {

                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        private class TrackingParameter
        {
            public TrackingParameter(string key, string value)
            {
                Key = key;
                Value = value;
            }

            public string Key { get; set; }
            public string Value { get; set; }
        }
    }
}
