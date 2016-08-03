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
    class LoginViewModel : ViewModelBase
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public ICommand LoginButtonClick
        {
            get
            {
                return new Command(() =>
                {
                    if (Username.IsNullOrEmpty() || Password.IsNullOrEmpty())
                    {
                        // invalid - tell the user
                        var view = this.GetRegisteredView();
                        view.DisplayAlert("Error", "Invalid Username or Password", "OK");
                    }
                    else
                    {
                        NavigationService.SetMainView<HomeWrapView>(true);
                    }
                });
            }
        }
    }
}
