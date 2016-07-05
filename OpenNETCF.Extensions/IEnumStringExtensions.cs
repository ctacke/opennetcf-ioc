#if (!WindowsCE) && (!WINDOWS_PHONE) && (!PCL)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenNETCF;

namespace System.Runtime.InteropServices.ComTypes
{
    public static class IEnumStringExtensions
    {
        private const int S_OK = 1;
        private const int S_NOTDONE = 0;
        private const int BufferSize = 100;

        public static List<string> GetStringList(this IEnumString enumerator)
        {
            Validate.Begin()
                .IsNotNull(enumerator)
                .Check();

            var browserNames = new List<string>();
            var stringNames = new List<string>();
            var count = 0;

            var countHandle = GCHandle.Alloc(count, GCHandleType.Pinned);

            try
            {
                var countPtr = countHandle.AddrOfPinnedObject();
                enumerator.Reset();

                var buffer = new string[BufferSize];
                var hresult = enumerator.Next(buffer.Length, buffer, countPtr);

                stringNames.AddRange(buffer);

                while (hresult == S_NOTDONE)
                {
                    hresult = enumerator.Next(buffer.Length, buffer, countPtr);
                    stringNames.AddRange(buffer);
                }

                if (hresult != S_OK)
                {
                    throw Marshal.GetExceptionForHR(hresult);
                }

                foreach (var item in stringNames)
                {
                    if (item != null)
                    {
                        browserNames.Add(item);
                    }
                }


            }
            finally
            {
                countHandle.Free();
            }

            return browserNames;
        }
    }
}
#endif