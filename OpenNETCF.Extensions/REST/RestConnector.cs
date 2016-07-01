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
#if ! WindowsCE
using System.Net.Security;
#endif

#if MONO
using Output = System.Console;
#else
using Output = System.Diagnostics.Debug;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace OpenNETCF.Web
{
    public class RestConnector
    {
        public bool LastCallFailed { get; set; }

        public int LastDataTime { get; set; }
        public int LastDataSize { get; set; }
        public int NumberErrors { get; set; }
        public int NumberTimeouts { get; set; }
        
        private object m_syncRoot = new object();
        private string m_address;
        private NetworkCredential m_credential;
        private bool m_ssl;
        private bool m_certOverridden;
        public bool IgnoreCertificateErrors { get; set; }

        public int Port { get; private set; }

        public string DeviceAddress
        {
            get { return m_address; }
            set
            {
                var address = value;
                if (!address.StartsWith("http", StringComparison.CurrentCultureIgnoreCase))
                {
                    address = string.Format("http{0}://{1}", m_ssl ? "s" : string.Empty, address);
                }

                var uri = new Uri(address, UriKind.Absolute);
                Port = uri.Port;
                m_address = string.Format("{0}://{1}:{2}", uri.Scheme, uri.Host, uri.Port);
            }
        }

        public RestConnector(string deviceAddress)
            : this(deviceAddress, null, null, false, false)
        {
        }

        public RestConnector(Uri deviceAddress)
            : this(deviceAddress.ToString(), null, null, false, false)
        {
        }

        public RestConnector(string deviceAddress, string username, string password, bool useSSL, bool ignoreCertificateErrors)
        {
#if!(WindowsCE || WINDOWS_PHONE || ANDROID || MONO)
            System.Net.Cache.HttpRequestCachePolicy policy = new System.Net.Cache.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore);
            HttpWebRequest.DefaultCachePolicy = policy;
#endif
            DeviceAddress = deviceAddress;

            if (!username.IsNullOrEmpty())
            {
                m_credential = new NetworkCredential(username, password);
            }
            m_ssl = useSSL;

            IgnoreCertificateErrors = ignoreCertificateErrors;
            m_certOverridden = false;
        }

        protected virtual CredentialCache GenerateCredentials()
        {
            return null;
        }

        public string Get(string directory)
        {
            return Get(directory, Timeout.Infinite);
        }

        private Dictionary<string, object> m_locks = new Dictionary<string, object>();

#if!(WindowsCE || WINDOWS_PHONE || ANDROID)
        public bool AcceptAllCertificates(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
#endif

        public string Get(string directory, int timeout)
        {
            return (string)GetData<string>(directory, timeout, GetResponseString);
        }

        public byte[] GetBytes(string directory, int timeout)
        {
            var result =  GetData<byte[]>(directory, timeout, GetResponseBytes);
            return (byte[])result;
        }

        private object GetData<T>(string directory, int timeout, Func<HttpWebResponse, T> predicate)
        {
            var start = Environment.TickCount;
            if (!Monitor.TryEnter(m_locks, 1000)) return null;

            try
            {
                var lockDirectory = directory.Replace('/', '\\');
                String fileInfo = string.Empty;
                try
                {
                    var index = lockDirectory.IndexOf('?');

                    if (index >= 0)
                    {
                        lockDirectory = lockDirectory.Substring(0, index);
                    }

                    fileInfo = lockDirectory;
                }
                catch
                {
                    fileInfo = directory;
                }

                if (!m_locks.Keys.Contains(fileInfo))
                {
                    m_locks.Add(fileInfo, new object());
                }

                if (!Monitor.TryEnter(m_locks[fileInfo]))
                {
                    Output.WriteLine("RestConnector Get can't get lock for: " + directory);
                    return string.Empty;
                }
                try
                {
                    directory = directory.Replace('\\', '/');
                    if (!directory.StartsWith("/"))
                    {
                        directory = "/" + directory;
                    }

                    string page = string.Format("{0}{1}", DeviceAddress, directory);
                    ServicePointManager.DefaultConnectionLimit = 50;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(page);

                    CredentialCache creds = GenerateCredentials();
                    if (creds != null)
                    {
                        request.Credentials = creds;
                    }
                    else
                    {
                        request.Credentials = m_credential;
                    }

                    if ((request.Credentials != null) && (IgnoreCertificateErrors) && (!m_certOverridden))
                    {
#if(WindowsCE || WINDOWS_PHONE || ANDROID)
                        System.Net.ServicePointManager.CertificatePolicy = new CFCertificatePolicy();
#else
                        ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(AcceptAllCertificates);
                        m_certOverridden = true;
#endif
                    }

#if !WINDOWS_PHONE
                    request.KeepAlive = false;

                    if (timeout > 0)
                    {
                        request.ReadWriteTimeout = timeout;
                        request.Timeout = timeout;
                    }
                    if (timeout <= 0)
                    {

                    }

#endif
                    try
                    {

                        using (var response = (HttpWebResponse)request.GetResponse())
                        {
                            LastCallFailed = false;
                            LastDataSize = (int)response.ContentLength;
                            return predicate(response);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is TimeoutException)
                        {
                            NumberTimeouts++;
                        }
                        else
                        {
                            NumberErrors++;
                        }
                        LastCallFailed = true;
                        LastDataTime = Environment.TickCount - start;
                        LastDataSize = 0;

                        Output.WriteLine("RestConnector Exception in GetData [{0}]: {1}", page, ex.Message);
                        request.Abort();
                        return default(T);
                    }
                }
                finally
                {
                    Monitor.Exit(m_locks[fileInfo]);
                    LastDataTime = Environment.TickCount - start;
                }
            }
            finally
            {
                Monitor.Exit(m_locks);
            }
        }

        private string SendData(string method, string directory, string data)
        {
            return SendData(method, directory, data, null, Timeout.Infinite);
        }

    private string SendData(string method, string directory, string data, string contentType, int timeout)
    {
        lock (m_syncRoot)
        {
            directory = directory.Replace('\\', '/');
            if (!directory.StartsWith("/"))
            {
                directory = "/" + directory;
            }

            string page = string.Format("{0}{1}", DeviceAddress, directory);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(page);
    #if !WINDOWS_PHONE
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
    #endif
            request.Method = method;

            CredentialCache creds = GenerateCredentials();
            if (creds != null)
            {
                request.Credentials = creds;
            }
            else
            {
                request.Credentials = m_credential;
            }

            if ((request.Credentials != null) && (IgnoreCertificateErrors) && (!m_certOverridden))
            {
    #if(WindowsCE || WINDOWS_PHONE || ANDROID || MONO)
                System.Net.ServicePointManager.CertificatePolicy = new CFCertificatePolicy();
    #else
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(AcceptAllCertificates);
                m_certOverridden = true;
    #endif
            }

            // turn our request string into a byte stream
            byte[] postBytes;

            if (data != null)
            {
                postBytes = Encoding.UTF8.GetBytes(data);
            }
            else
            {
                postBytes = new byte[0];
            }

    #if !WINDOWS_PHONE
            if (timeout < 0)
            {
                request.ReadWriteTimeout = timeout;
                request.Timeout = timeout;
            }

            request.ContentLength = postBytes.Length;
            request.KeepAlive = false;
    #endif
            if (contentType.IsNullOrEmpty())
            {
                request.ContentType = "application/x-www-form-urlencoded";
            }
            else
            {
                request.ContentType = contentType;
            }

            try
            {
                Stream requestStream = request.GetRequestStream();

                // now send it
                requestStream.Write(postBytes, 0, postBytes.Length);
                requestStream.Close();


                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    return GetResponseString(response);
                }
            }
            catch (Exception ex)
            {
                Output.WriteLine("RestConnector Exception in SendData (string) [{0}]: {1}", page, ex.Message);
                request.Abort();
                return string.Empty;
            }
        }
    }

        private string SendData(string method, string directory, byte[] data, int timeout)
        {
            lock (m_syncRoot)
            {
                directory = directory.Replace('\\', '/');
                if (!directory.StartsWith("/"))
                {
                    directory = "/" + directory;
                }

                string page = string.Format("{0}{1}", DeviceAddress, directory);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(page);
#if !WINDOWS_PHONE
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
#endif
                request.Method = method;

                CredentialCache creds = GenerateCredentials();
                if (creds != null)
                {
                    request.Credentials = creds;
                }
                else
                {
                    request.Credentials = m_credential;
                }

                if ((request.Credentials != null) && (IgnoreCertificateErrors) && (!m_certOverridden))
                {
#if(WindowsCE || WINDOWS_PHONE || ANDROID || MONO)
                    System.Net.ServicePointManager.CertificatePolicy = new CFCertificatePolicy();
#else
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(AcceptAllCertificates);
                    m_certOverridden = true;
#endif
                }

#if !WINDOWS_PHONE
                if (timeout < 0)
                {
                    request.ReadWriteTimeout = timeout;
                    request.Timeout = timeout;
                }

                request.ContentLength = data.Length;
                request.KeepAlive = false;
#endif
                request.ContentType = "application/octet-stream";

                try
                {
                    Stream requestStream = request.GetRequestStream();

                    // now send it
                    requestStream.Write(data, 0, data.Length);
                    requestStream.Close();


                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        return GetResponseString(response);
                    }
                }
                catch (Exception ex)
                {
                    Output.WriteLine("RestConnector Exception in SendData (binary) [{0}]: {1}", page, ex.Message);
                    request.Abort();
                    return string.Empty;
                }
            }
        }

        public string Post(string directory, byte[] data, int timeout)
        {
            return SendData("POST", directory, data, timeout);
        }

        public string Post(string directory, string data, string contentType, int timeout)
        {
            return SendData("POST", directory, data, contentType, timeout);
        }

        public string Post(string directory, string data, int timeout)
        {
            return Post(directory, data, null, timeout);
        }

        public string Post(string directory, XElement data, int timeout)
        {
            return Post(directory, data.ToString(), "text/xml", timeout);
        }

        public string Post(string directory, XElement data)
        {
            return Post(directory, data.ToString(), "text/xml", Timeout.Infinite);
        }

        public string Post(string directory, string data)
        {
            return SendData("POST", directory, data);
        }

        public string Put(string directory, string data, string contentType, int timeout)
        {
            return SendData("PUT", directory, data, contentType, timeout);
        }

        public string Put(string directory, string data, int timeout)
        {
            return Put(directory, data, null, timeout);
        }

        public string Put(string directory, XElement data, int timeout)
        {
            return Put(directory, data.ToString(), timeout);
        }

        public string Put(string directory, XElement data)
        {
            return Put(directory, data.ToString());
        }

        public string Put(string directory, string data)
        {
            return SendData("PUT", directory, data);
        }

        public string Delete(string directory, int timeout)
        {
            return SendData("DELETE", directory, null, null, timeout);
        }

        public string Delete(string directory)
        {
            return SendData("DELETE", directory, null);
        }

        public string Delete(string directory, XElement data, int timeout)
        {
            return SendData("DELETE", directory, data.ToString(), "text/xml", timeout);
        }

        private byte[] GetResponseBytes(HttpWebResponse response)
        {
            var data = new List<byte>(8192);
            byte[] buf = new byte[8192];

            using (var stream = response.GetResponseStream())
            {
                int count = 0;

                do
                {
                    count = stream.Read(buf, 0, buf.Length);

                    if (count != 0)
                    {
                        data.AddRange(buf.Take(count));
                    }
                } while (count > 0);

                response.Close();
            }

            return data.ToArray();
        }

        private string GetResponseString(HttpWebResponse response)
        {
            var start = Environment.TickCount;
            StringBuilder sb = new StringBuilder(4096);

            byte[] buf = new byte[8192];

            using (var stream = response.GetResponseStream())
            {
                string result = null;
                int count = 0;

                do
                {
                    count = stream.Read(buf, 0, buf.Length);

                    if (count != 0)
                    {
                        // look for a UTF8 header
                        if ((buf[0] == 0xEF) && (buf[1] == 0xBB) && (buf[2] == 0xBF))
                        {
                            result = Encoding.UTF8.GetString(buf, 3, count - 3);
                        }
                        else
                        {
                            result = Encoding.UTF8.GetString(buf, 0, count);
                        }
                        sb.Append(result);
                    }
                } while (count > 0);

                response.Close();
            }

            return sb.ToString();
        }
    }

    internal class CFCertificatePolicy : ICertificatePolicy
    {
        public enum CertProblem : uint
        {
            None = 0,
            Expired = 0x800b0101,
            ValidityPeriodNesting = 0x800b0102,
            Role = 0x800b0103,
            PathLength = 0x800b0104,
            Critical = 0x800b0105,
            Purpose = 0x800b0106,
            IssuerChaining = 0x800b0107,
            Malformed = 0x800b0108,
            UntrustedRoot = 0x800b0109,
            Chaining = 0x800b010a,
            Revoked = 0x800b010c,
            UntrustedTestRoot = 0x800b010d,
            RevocationFailure = 0x800b010e,
            NameMismatch = 0x800b010f,
            WrongUsage = 0x800b0110,
            UntrustedCA = 0x800b0111,
        }

        public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
        {
            var problem = (CertProblem)certificateProblem;

            switch (problem)
            {
                case CertProblem.None:
                    return true;
                case CertProblem.Chaining:
                    Output.WriteLine("Certificate Chaining Problem");
                    return true;
                default:
                    return false;
            }
        }
    }
}
