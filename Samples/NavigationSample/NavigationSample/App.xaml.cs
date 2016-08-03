using NavigationSample.ViewModels;
using NavigationSample.Views;
using OpenNETCF.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace NavigationSample
{
    public partial class App : Xamarin.Forms.Application
    {
        public App()
        {
            InitializeComponent();

            RegisterViews();

            NavigationService.SetMainView<HomeView>(true);
        }

        private void RegisterViews()
        {
            NavigationService.Register<HomeView, HomeViewModel>();
            NavigationService.Register<DetailsView, DetailsViewModel>();
        }
    }
}
