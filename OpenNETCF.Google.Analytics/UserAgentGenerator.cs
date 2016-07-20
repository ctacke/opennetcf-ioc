using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OpenNETCF.GA
{
    public static class UserAgentGenerator
    {
        public static string GetPlatformUserAgent()
        {
            switch (Device.OS)
            {
                case TargetPlatform.Android:
                    return GetAndroidUserAgent();
                case TargetPlatform.iOS:
                    return GetiOSUserAgent();
                case TargetPlatform.Windows:
                    return GetWindowsUserAgent();
                case TargetPlatform.WinPhone:
                    return GetWinPhoneUserAgent();
                default:
                    return GetDefaultAgent();
            }
        }

        private static string GetAndroidUserAgent()
        {
            // Mozilla/5.0 (Linux; <Android Version>; <Build Tag etc.>) AppleWebKit/<WebKit Rev> (KHTML, like Gecko) Chrome/<Chrome Rev> Mobile Safari/<WebKit Rev>

            // Mozilla/5.0 (Linux; <Android Version>; <Build Tag etc.>) AppleWebKit/<WebKit Rev>(KHTML, like Gecko) Chrome/<Chrome Rev> Safari/<WebKit Rev>

            // for now just return a WebView user agent, until I come up with something better
            //return "Mozilla / 5.0(Linux; Android 5.1.1; Nexus 5 Build / LMY48B; wv) AppleWebKit / 537.36(KHTML, like Gecko) Version / 4.0 Chrome / 43.0.2357.65 Mobile Safari/ 537.36";
            return "Mozilla/5.0 (Linux; U; Android; en-us) AppleWebKit/999+ (KHTML, like Gecko) Safari/999.9 [ONCF_AN / v1.0]";
        }

        private static string GetiOSUserAgent()
        {
            switch (Device.Idiom)
            {
                case TargetIdiom.Phone:
                    return "User-Agent: Mozilla/5.0 (iPhone; U; CPU OS 4_3_2 like Mac OS X; en-us) AppleWebKit/533.17.9 (KHTML, like Gecko) Mobile [ONCF_AN / v1.0]";
                case TargetIdiom.Tablet:
                    return "User-Agent: Mozilla/5.0 (iPad; U; CPU OS 4_3_2 like Mac OS X; en-us) AppleWebKit/533.17.9 (KHTML, like Gecko) Mobile [ONCF_AN / v1.0]";
                case TargetIdiom.Desktop:
                default:
                    return "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_3) AppleWebKit/537.75.14 (KHTML, like Gecko) Version/7.0.3 Safari/7046A194A [ONCF_AN / v1.0]";
            }
        }

        private static string GetWindowsUserAgent()
        {
            // just pretend we're the Edge browser for now
            return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246 [ONCF_AN / v1.0]";
        }

        private static string GetWinPhoneUserAgent()
        {
            return "Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0)";
        }

        private static string GetDefaultAgent()
        {
            return string.Empty;
        }
    }
}
