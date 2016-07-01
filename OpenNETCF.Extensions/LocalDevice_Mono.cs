using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.IO;

#if MONO
using Mono.Unix.Native;

namespace System
{
    public static partial class LocalDevice
    {
        private class MonoInterface : ILocalInterface
        {
            private MemInfo m_meminfo;

            internal MonoInterface()
            {
                m_meminfo = new MemInfo();
            }

            public void SetHostName(string name)
            {
                throw new NotSupportedException();
            }

            public void SetLocalTime(DateTime time)
            {
                throw new NotSupportedException();
            }

            public void SetSystemTime(DateTime time)
            {
                time.ToEpochMilliseconds();
                long unixTime = time.ToEpochMilliseconds();
                Syscall.stime(ref unixTime);
            }

            public string GetHostName()
            {
                return Dns.GetHostName();
            }

            public System.Net.NetworkInformation.PhysicalAddress GetMACAddress()
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                if (interfaces == null)
                {
                    Console.WriteLine("!! No network interface list");
                    return PhysicalAddress.None;
                }

                var intf = from i in interfaces
                           where (i.NetworkInterfaceType == NetworkInterfaceType.Ethernet || i.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                       && i.OperationalStatus == OperationalStatus.Up
                           select i;

                var current = intf.FirstOrDefault();

                if (current == null)
                {
                    Console.WriteLine("!! No current network interface");

                    foreach (var i in interfaces)
                    {
                        Console.WriteLine(string.Format(" > {0} {1} {2}", i.Name, i.NetworkInterfaceType, i.OperationalStatus));
                    }

                    return PhysicalAddress.None;
                }

                return current.GetPhysicalAddress();
            }

            public bool Is64BitOperatingSystem()
            {
                throw new NotImplementedException();
            }

            public void Restart()
            {
                Console.WriteLine("Restarting device...");
                try
                {
                    Process.Start("su", "-c reboot");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to restart device: " + ex.Message);
                }
            }

            public ulong GetTotalMemory()
            {
                try
                {
                    m_meminfo.Refresh();
                    if (m_meminfo.Values.ContainsKey("MemTotal"))
                    {
                        return m_meminfo.Values["MemTotal"] * 1024;
                    }
                    return 0;
                }
                catch
                {
                    return 0;
                }
            }

            public ulong GetFreeMemory()
            {
                try
                {
                    m_meminfo.Refresh();
                    if (m_meminfo.Values.ContainsKey("MemFree"))
                    {
                        return m_meminfo.Values["MemFree"] * 1024;
                    }
                    return 0;
                }
                catch (Exception)
                {
                    return 0;
                }
            }

            private long m_lastUser = -1;
            private long m_lastNice = -1;
            private long m_lastSystem = -1;
            private long m_lastIdle = -1;

            public decimal GetCpuLoad()
            {
                try
                {
                    var line = File.ReadLines("/proc/stat").FirstOrDefault();
                    if (line == null)
                    {
                        return -1;
                    }

                    var elements = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (elements.Length < 5)
                    {
                        return -2;
                    }

                    // pull ticks
                    var firstRun = m_lastUser < 0;

                    // convert all new values
                    var user = Convert.ToInt64(elements[1]);
                    var nice = Convert.ToInt64(elements[2]);
                    var system = Convert.ToInt64(elements[3]);
                    var idle = Convert.ToInt64(elements[4]);

                    //                Console.WriteLine(string.Format("STAT: {0} {1} {2} {3}", user, nice, system, idle));

                    // subtract from last calculation (we determine load over the measurement period)
                    var user2 = user - m_lastUser;
                    m_lastUser = user;
                    var nice2 = nice - m_lastNice;
                    m_lastNice = nice;
                    var system2 = system - m_lastSystem;
                    m_lastSystem = system;
                    var idle2 = idle - m_lastIdle;
                    m_lastIdle = idle;

                    if (firstRun) return 0;

                    var busy = user2 + nice2 + system2;
                    var total = busy + idle2;

                    if (total == 0) return 0;

                    return Math.Round((busy / (decimal)total) * 100m, 0);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return -3;
                }
            }
        }

        private class MemInfo
        {
//            root@WR-IntelligentDevice:~# cat /proc/meminfo
//            MemTotal:         878724 kB
//            MemFree:          691388 kB
//            Buffers:           11728 kB
//            Cached:           120344 kB
//            SwapCached:            0 kB
//            Active:            52068 kB
//            Inactive:         109064 kB
//            Active(anon):      29252 kB
//            Inactive(anon):      220 kB
//            Active(file):      22816 kB
//            Inactive(file):   108844 kB
//            Unevictable:           0 kB
//            Mlocked:               0 kB
//            HighTotal:             0 kB
//            HighFree:              4 kB
//            LowTotal:         878724 kB
//            LowFree:          691384 kB
//            SwapTotal:             0 kB
//            SwapFree:              0 kB
//            Dirty:               152 kB
//            Writeback:             0 kB
//            AnonPages:         29064 kB
//            Mapped:            15204 kB
//            Shmem:               412 kB
//            Slab:              11224 kB
//            SReclaimable:       5476 kB
//            SUnreclaim:         5748 kB
//            KernelStack:         824 kB
//            PageTables:         1096 kB
//            NFS_Unstable:          0 kB
//            Bounce:                0 kB
//            WritebackTmp:          0 kB
//            CommitLimit:      439360 kB
//            Committed_AS:     115244 kB
//            VmallocTotal:     122880 kB
//            VmallocUsed:        2300 kB
//            VmallocChunk:     109060 kB
//            HugePages_Total:       0
//            HugePages_Free:        0
//            HugePages_Rsvd:        0
//            HugePages_Surp:        0
//            Hugepagesize:       4096 kB
//            DirectMap4k:       12280 kB
//            DirectMap4M:      897024 kB
            public MemInfo()
            {
                Values = new Dictionary<string, ulong>(StringComparer.InvariantCultureIgnoreCase);
                Refresh();
            }

            public Dictionary<string, ulong> Values { get; private set; }

            public void Refresh()
            {
                try
                {
                    var lines = File.ReadAllLines("/proc/meminfo");

                    foreach (var line in lines)
                    {
                        try
                        {
                            var index = line.IndexOf(':');
                            if (index < 0)
                            {
                                continue;
                            }
                            var name = line.Substring(0, index);

                            var valueString = line.Substring(index + 1).Trim();
                            if (valueString.EndsWith("kB"))
                            {                    
                                valueString = valueString.Substring(0, valueString.Length - 3);
                            }
                            var value = Convert.ToUInt64(valueString);

                            if (Values.ContainsKey(name))
                            {
                                Values[name] = value;
                            }
                            else
                            {
                                Values.Add(name, value);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("MemInfo.Refresh(i): " + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("MemInfo.Refresh(o): " + ex.Message);
                }
            }

        }

    }
}
#endif
