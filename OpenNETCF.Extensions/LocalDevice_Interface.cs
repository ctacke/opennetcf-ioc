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
    using OpenNETCF.Net.NetworkInformation;
#else
    using System.Net.NetworkInformation;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.InteropServices;

namespace System
{
    public static partial class LocalDevice
    {
        private static Random m_rand;

        public static Random Random
        {
            get 
            {
                // lazy load
                if (m_rand == null)
                {
                    m_rand = new Random(Environment.TickCount);
                }

                return m_rand; 
            }
        }

        private static ILocalInterface m_local;

        private interface ILocalInterface
        {
            void SetHostName(string name);
            string GetHostName();
            void SetLocalTime(DateTime time);
            void SetSystemTime(DateTime time);
            PhysicalAddress GetMACAddress();
            bool Is64BitOperatingSystem();
            void Restart();
            ulong GetTotalMemory();
            ulong GetFreeMemory();
            decimal GetCpuLoad();
        }
    }
}
