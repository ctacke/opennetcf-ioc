using System;
using System.Net;
using System.Threading;
using System.IO;

namespace System.Net
{
    public static class HttpWebRequestExtensions
    {
        public static HttpWebResponse GetResponse(this HttpWebRequest request, bool throwOnError)
        {
            try
            {
                return (HttpWebResponse)request.GetResponse(true);
            }
            catch (WebException we)
            {
                if ((!throwOnError) && (we.Response != null))
                {
                    return (HttpWebResponse)we.Response;
                }
                throw;
            }
        }

#if WINDOWS_PHONE
        private const int DefaultRequestTimeout = 5000;

        public static HttpWebResponse GetResponse(this HttpWebRequest request)
        {
            var dataReady = new AutoResetEvent(false);
            HttpWebResponse response = null;
            var callback = new AsyncCallback(delegate(IAsyncResult asynchronousResult)
            {
                response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
                dataReady.Set();
            });

            request.BeginGetResponse(callback, request);

            if (dataReady.WaitOne(DefaultRequestTimeout))
            {
                return response;
            }

            return null;
        }

        public static Stream GetRequestStream(this HttpWebRequest request)
        {
            var dataReady = new AutoResetEvent(false);
            Stream stream = null;
            var callback = new AsyncCallback(delegate(IAsyncResult asynchronousResult)
            {
                stream = (Stream)request.EndGetRequestStream(asynchronousResult);
                dataReady.Set();
            });

            request.BeginGetRequestStream(callback, request);
            if (!dataReady.WaitOne(DefaultRequestTimeout))
            {
                return null;
            }

            return stream;
        }
#endif
    }
}
