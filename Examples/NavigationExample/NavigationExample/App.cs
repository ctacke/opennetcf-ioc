using NavigationExample.ViewModels;
using NavigationExample.Views;
using OpenNETCF.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace NavigationExample
{
    public class App : Application
    {
        public App()
        {
            RegisterViews();

            NavigationService.SetMainView<LoginView>(false);
        }

        private void RegisterViews()
        {
            NavigationService.Register<LoginView, LoginViewModel>();
            NavigationService.Register<HomeWrapView, HomeWrapViewModel>();
            NavigationService.Register<HomeView, HomeViewModel>();
            NavigationService.Register<MenuView, MenuViewModel>();
            NavigationService.Register<DetailsView, DetailsViewModel>();
            NavigationService.Register<SettingsView, SettingsViewModel>();
        }
    }
}
