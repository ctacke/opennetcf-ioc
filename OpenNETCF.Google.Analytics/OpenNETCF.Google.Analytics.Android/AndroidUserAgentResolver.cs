using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNETCF.GA
{
    public class AndroidUserAgentResolver : IUserAgentResolver
    {
        private string m_userAgent;

        public string GetUserAgent()
        {
            try
            {
                // get it once and only once - it's not going to change
                if (m_userAgent == null)
                {
                    m_userAgent = Java.Lang.JavaSystem.GetProperty("http.agent");
                }

                return m_userAgent;
            }
            catch
            {
                return null;
            }
        }
    }
}
