using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenNETCF.IoC.UI
{
    internal static class Extensions
    {
        public static void BeginInvokeIfRequired(this Control control, Action a)
        {
            if (control.InvokeRequired)
            {              
                control.BeginInvoke(a);
            }
            else
            {
               a();
            }
        }

        public static void InvokeIfRequired(this Control control, Action a)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(a);
            }
            else
            {
                a();
            }
        }
    }
}
