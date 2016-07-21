using OpenNETCF.GA;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OpenNETCF.IoC
{
    public class ViewTypeNotRegisteredException : Exception
    {
        public ViewTypeNotRegisteredException(Type viewType)
            : base(string.Format("View type '{0}' not registered", viewType.Name))
        {
        }
    }
}