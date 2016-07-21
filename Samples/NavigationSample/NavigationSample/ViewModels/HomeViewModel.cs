using OpenNETCF.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using NavigationSample.Views;

namespace NavigationSample.ViewModels
{
    class HomeViewModel : IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand ShowDetailsCommand
        {
            get
            {
                return new Command(() =>
                {
                    NavigationService.NavigateForward<DetailsView>(true);
                });
            }
        }
    }
}
