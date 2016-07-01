#if ANDROID

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading;

namespace OpenNETCF
{
    public class UIInvoker : DisposableBase
    {
        private Activity m_activity = new Activity();

        public void Invoke(Delegate method)
        {
            m_activity.RunOnUiThread(delegate
            {
                method.DynamicInvoke(null);
            });
        }

        public void BeginInvoke(Delegate method)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                m_activity.RunOnUiThread(delegate
                {
                    method.DynamicInvoke(null);
                });
            });
        }

        public void Invoke(Delegate method, object[] @params)
        {
            m_activity.RunOnUiThread(delegate
            {
                method.DynamicInvoke(@params);
            });
        }

        public void BeginInvoke(Delegate method, object[] @params)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                m_activity.RunOnUiThread(delegate
                {
                    method.DynamicInvoke(@params);
                });
            });
        }
    }
}

#endif
