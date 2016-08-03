using OpenNETCF.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NavigationExample.ViewModels
{
    class ViewModelBase : IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged.Fire(this, propertyName);
        }
    }
}
