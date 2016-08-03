using NavigationExample.Views;
using OpenNETCF.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace NavigationExample.ViewModels
{
    class MenuViewModel : ViewModelBase
    {
        public ICommand SettingsClicked
        {
            get
            {
                return new Command(() =>
                {
                    NavigationService.NavigateForward<SettingsView>();
                });
            }
        }
    }
}
