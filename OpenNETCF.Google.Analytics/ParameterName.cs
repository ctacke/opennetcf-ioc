using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;

using Xamarin.Forms;

namespace OpenNETCF.GA
{
    internal static class ParameterName
    {
        public const string AnonymizeIP = "aip";
        public const string DataSource = "ds";
        public const string ApplicationName = "an";
        public const string ScreenName = "cd";
        public const string ApplicationVersion = "av";
        public const string ApplicationID = "aid";
        public const string ApplicationInstallerID = "aiid";
        public const string SessionControl = "sc";
        public const string EventCategory = "ec";
        public const string EventAction = "ea";
        public const string EventLabel = "el";
        public const string EventValue = "ev";

        public const string TimingCategory = "utc";
        public const string TimingVariable = "utv";
        public const string TimingTime = "utt";
        public const string TimingLabel = "utl";
    }
}
