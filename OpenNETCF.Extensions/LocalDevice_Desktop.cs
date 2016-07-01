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
#if DESKTOP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Diagnostics;

namespace System
{
    public static partial class LocalDevice
    {
        private class WindowsDesktopInterface : ILocalInterface
        {
            private PerformanceCounter m_cpuLoad;

            public WindowsDesktopInterface()
            {
                // this requires two calls to calculate, so prime the first here
                var temp = GetCpuLoad();
            }

            public void SetHostName(string name)
            {
                throw new NotSupportedException();
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
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                var intf = from i in interfaces
                           where (i.NetworkInterfaceType == NetworkInterfaceType.Ethernet || i.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                       && i.OperationalStatus == OperationalStatus.Up
                           select i;

                var current = intf.FirstOrDefault();
                if (current == null)
                {
                    // no network adapter found
                    return new PhysicalAddress(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                }
                return current.GetPhysicalAddress();
            }

            public bool Is64BitOperatingSystem()
            {
                if (IntPtr.Size == 8)  // 64-bit programs run only on Win64
                {
                    return true;
                }
                else  // 32-bit programs run on both 32-bit and 64-bit Windows
                {
                    // Detect whether the current process is a 32-bit process 
                    // running on a 64-bit system.
                    bool flag;
                    return ((DoesWin32MethodExist("kernel32.dll", "IsWow64Process") &&
                        IsWow64Process(GetCurrentProcess(), out flag)) && flag);
                }
            }

            private bool DoesWin32MethodExist(string moduleName, string methodName)
            {
                IntPtr moduleHandle = GetModuleHandle(moduleName);
                if (moduleHandle == IntPtr.Zero)
                {
                    return false;
                }
                return (GetProcAddress(moduleHandle, methodName) != IntPtr.Zero);
            }

            public void Restart()
            {
                // TODO:
                Debug.WriteLine("Device Restart not implemented on this platform.");
            }

            public ulong GetTotalMemory()
            {
                var ms = new MEMORYSTATUSEX();
                ms.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
                GlobalMemoryStatusEx(ref ms);
                return ms.ullTotalPhys;
            }

            public ulong GetFreeMemory()
            {
                var ms = new MEMORYSTATUSEX();
                ms.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
                GlobalMemoryStatusEx(ref ms);
                return ms.ullAvailPhys;
            }

            public decimal GetCpuLoad()
            {
                try
                {
                    if (m_cpuLoad == null)
                    {
                        m_cpuLoad = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    }
                    return (decimal)m_cpuLoad.NextValue();
                }
                catch (Exception)
                {
                    // if the perf counter is missing or disabled (like in XP)
                    return 101;
                }

            }

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool SetSystemTime(ref SYSTEMTIME st);

            [DllImport("kernel32.dll")]
            static extern IntPtr GetCurrentProcess();

            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            static extern IntPtr GetModuleHandle(string moduleName);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            static extern IntPtr GetProcAddress(IntPtr hModule,
                [MarshalAs(UnmanagedType.LPStr)]string procName);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);

            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

            internal struct SYSTEMTIME
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

            private struct MEMORYSTATUSEX
            {
                public uint dwLength;
                public uint dwMemoryLoad;
                public ulong ullTotalPhys;
                public ulong ullAvailPhys;
                public ulong ullTotalPageFile;
                public ulong ullAvailPageFile;
                public ulong ullTotalVirtual;
                public ulong ullAvailVirtual;
                public ulong ullAvailExtendedVirtual;
            }
        }
    }
}
#endif