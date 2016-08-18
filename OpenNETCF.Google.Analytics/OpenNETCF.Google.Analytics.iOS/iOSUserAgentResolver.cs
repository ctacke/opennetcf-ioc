using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

namespace OpenNETCF.GA
{
    public class GA
    {
        public static void Init()
        {
            Xamarin.Forms.DependencyService.Register<iOSUserAgentResolver>();
        }
    }

    public class iOSUserAgentResolver : IUserAgentResolver
    {
        private string m_userAgent;

        public string GetUserAgent()
        {
            try
            {
                // get it once and only once - it's not going to change
                if (m_userAgent == null)
                {
                    using (var webView = new UIWebView(CGRect.Empty))
                    {
                        m_userAgent = webView.EvaluateJavascript("navigator.userAgent");
                    }
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
