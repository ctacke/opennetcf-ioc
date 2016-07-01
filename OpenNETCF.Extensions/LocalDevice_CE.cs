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
#if WindowsCE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.InteropServices;
using OpenNETCF.Net.NetworkInformation;

namespace System
{
    public static partial class LocalDevice
    {
        private class WinCEInterface : ILocalInterface
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct WSAData
            {
                public Int16 version;
                public Int16 highVersion;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
                public String description;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
                public String systemStatus;

                public Int16 maxSockets;
                public Int16 maxUdpDg;
                public IntPtr vendorInfo;
            }

            [DllImport("ws2.dll", SetLastError = true)]
            static extern int WSAStartup(short wVersionRequested, ref WSAData wsaData);

            [DllImport("ws2.dll", SetLastError = true)]
            static extern int WSACleanup();

            [DllImport("ws2.dll", SetLastError = true)]
            private static extern int sethostname(byte[] pName, int cName);

            [DllImport("coredll.dll", SetLastError = true)]
            internal static extern bool SetSystemTime(ref SYSTEMTIME st);

            public const int HIGH_VERSION = 2;
            public const int LOW_VERSION = 2;
            public const short WORD_VERSION = 36;

            public void SetHostName(string name)
            {
                // this takes a long time (often > 30 seconds)
                WSAData data = new WSAData();

                data.highVersion = HIGH_VERSION;
                data.version = LOW_VERSION;

                var result = WSAStartup(WORD_VERSION, ref data);

                var nameBytes = Encoding.ASCII.GetBytes(name + '\0');
                result = sethostname(nameBytes, nameBytes.Length);

                WSACleanup();
            }

            public void SetLocalTime(DateTime time)
            {
                SetSystemTime(time.ToUniversalTime());
            }

            public void SetSystemTime(DateTime time)
            {
                var st = new SYSTEMTIME()
                {
                    Day = (ushort)time.Day,
                    Month = (ushort)time.Month,
                    Year = (ushort)time.Year,
                    Hour = (ushort)time.Hour,
                    Minute = (ushort)time.Minute,
                    Second = (ushort)time.Second,
                    Millisecond = (ushort)time.Millisecond
                };

                SetSystemTime(ref st);
            }

            public string GetHostName()
            {
                return Dns.GetHostName();
            }

            public PhysicalAddress GetMACAddress()
            {
                var intf = from i in NetworkInterface.GetAllNetworkInterfaces()
                           where i.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                           && i.OperationalStatus == OperationalStatus.Up
                           select i;

                var current = intf.FirstOrDefault();
                return current.GetPhysicalAddress();
            }


            public bool Is64BitOperatingSystem()
            {
                return false;
            }

            private struct SYSTEMTIME
            {
                public ushort Year;
                public ushort Month;
                public ushort DayOfWeek;
                public ushort Day;
                public ushort Hour;
                public ushort Minute;
                public ushort Second;
                public ushort Millisecond;
            };
        }
    }
}
#endif
