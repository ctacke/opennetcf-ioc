// -------------------------------------------------------------------------------------------------------
// LICENSE INFORMATION
//
// - This software is licensed under the MIT shared source license.
// - The "official" source code for this project is maintained at http://oncfext.codeplex.com
//
// Copyright (c) 2010 OpenNETCF Consulting
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
// associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial 
// portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT 
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
// -------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace System
{
    public static class DateTimeExtensions
    {
        private static DateTime TimeTMinimumDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Minutes elaped between the given DateTime and DateTime.Now
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static double AgeInMinutes(this DateTime t)
        {
            return (DateTime.Now - t).TotalMinutes;
        }

        /// <summary>
        /// Gets the total minutes since midmight of the same day
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static double MinutesSinceMidnight(this DateTime t)
        {
            return (t - t.Date).TotalMinutes;
        }

        public static double To_time_t_Ticks(this DateTime t)
        {
            return (t - TimeTMinimumDate).TotalMilliseconds;
        }

        public static DateTime ToDateTimeFromEpochMilliseconds(this long mils)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(mils);
        }

        public static long ToEpochMilliseconds(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalMilliseconds);
        }

        public static int ToJulianDay(this DateTime date)
        {
            return ((date.Year - 2000) * 1000) + date.DayOfYear;
        }

        public static DayOfWeek PreviousDay(this DayOfWeek d)
        {
            switch (d)
            {
                case DayOfWeek.Sunday:
                    return DayOfWeek.Saturday;
                default:
                    return (DayOfWeek)(d - 1);
            }
        }

        public static DayOfWeek NextDay(this DayOfWeek d)
        {
            switch (d)
            {
                case DayOfWeek.Saturday:
                    return DayOfWeek.Sunday;
                default:
                    return (DayOfWeek)(d + 1);
            }
        }
    }
}
