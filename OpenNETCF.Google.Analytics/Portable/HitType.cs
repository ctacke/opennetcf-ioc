using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;

using Xamarin.Forms;

namespace OpenNETCF.GA
{
    internal static class HitType
    {
        public const string PageView = "pageview";
        public const string ScreenView = "screenview";
        public const string Event = "event";
        public const string Transaction = "transaction";
        public const string Item = "item";
        public const string Social = "social";
        public const string Exception = "exception";
        public const string Timing = "timing";
    }
}
