using OpenNETCF.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace NavigationSample.ViewModels
{
    class DetailsViewModel : IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand BackCommand
        {
            get
            {
                return new Command(() =>
                {
                    NavigationService.NavigateBack(true);
                });
            }
        }
    }
}
