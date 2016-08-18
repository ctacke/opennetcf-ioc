using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNETCF.GA
{
    public static class GA
    {
        public static void Init()
        {
            Xamarin.Forms.DependencyService.Register<AndroidUserAgentResolver>();
        }
    }
}
