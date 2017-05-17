using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public static class Extensions
    {
        public static void InvokeIfRequired<T>(this T control, Action<T> action)
            where T : Control
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new Action(() => action(control)));
            }
            else
            {
                action(control);
            }
        }

        public static void BeginInvokeIfRequired<T>(this T control, Action<T> action)
            where T : Control
        {
            if (control.InvokeRequired)
            {
                control.BeginInvoke(new Action(() => action(control)));
            }
            else
            {
                action(control);
            }
        }

        public static void InvokeUserIfRequired<T>(this T control, Action<T> action)
           where T : UserControl
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new Action(() => action(control)));
            }
            else
            {
                action(control);
            }
        }
    }
}
