using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace FormsExample.ViewModels
{
    class MainPageViewModel
    {
        public object[] StatusStrings
        {
            get
            {
                return new string[]
                {
                    "Status Item 1",
                    "Status Item 2",
                    "Status Item 3",
                };
            }
        }

        public ICommand StatusClicked
        {
            get
            {
                return new Command((item) =>
                {
                    Debug.WriteLine("Status click: " + item.ToString());
                });
            }
        }
    }
}
