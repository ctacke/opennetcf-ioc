using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public static class TimeSpanExtensions
    {
        public static string ToTimeString(this TimeSpan ts)
        {
            return string.Format("{0:00}:{1:00}:{2:00}",
                           (int)ts.TotalHours,
                                ts.Minutes,
                                ts.Seconds);
        }
    }
}
