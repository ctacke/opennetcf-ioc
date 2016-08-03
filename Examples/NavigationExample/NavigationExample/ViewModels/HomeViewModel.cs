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
    class HomeViewModel : ViewModelBase
    {
        public ICommand ItemClicked
        {
            get
            {
                return new Command(() =>
                {
                    NavigationService.NavigateForward<DetailsView>();
                });
            }
        }

        public ICommand MenuClicked
        {
            get
            {
                return new Command(() =>
                {
                    // get the wrapper MasterDetailPage
                    var homeWrap = NavigationService.GetView<HomeWrapView>();
                    homeWrap.IsPresented = true;
                });
            }
        }
    }
}
