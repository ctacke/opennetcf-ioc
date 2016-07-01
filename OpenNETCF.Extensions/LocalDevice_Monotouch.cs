#if ANDROID
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
        private class MonotouchInterface : ILocalInterface
        {
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
                throw new NotSupportedException();
            }

            public string GetHostName()
            {
                throw new NotSupportedException();
            }

#region ILocalInterface Members


            public Net.NetworkInformation.PhysicalAddress GetMACAddress()
            {
                throw new NotImplementedException();
            }

            #endregion

            public bool Is64BitOperatingSystem()
            {
                throw new NotImplementedException();
            }
        }
    }
}
#endif
