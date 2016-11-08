using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OpenNETCF.IoC.MVVM
{
    public static class ExtensionMethods
    {
        public static IViewModel GetRegisteredViewModel(this Page view)
        {
            var viewType = view.GetType();
            return NavigationService.GetViewModelForView(viewType);
        }

        public static Page GetRegisteredView(this IViewModel viewModel)
        {
            var viewModelType = viewModel.GetType();
            return NavigationService.GetViewForViewModel(viewModelType);
        }
    }
}
