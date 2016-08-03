using OpenNETCF.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace NavigationExample.Views
{
    public partial class HomeView : ContentPage
    {
        public HomeView()
        {
            InitializeComponent();

            // since the navigationService wasn't used to create this instance, we have to manually set the BindingContext
            this.BindingContext = this.GetRegisteredViewModel();

//            this.BindingContext = NavigationService.
        }
    }
}
