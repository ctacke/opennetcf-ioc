using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace FormsExample.ViewModels
{
    class MainPageViewModel : INotifyPropertyChanged
    {
        private List<string> m_items = new List<string>();

        public event PropertyChangedEventHandler PropertyChanged;

        public object[] StatusStrings
        {
            get
            {
                return m_items.ToArray();
            }
        }

        public bool HasNotifications
        {
            get { return m_items.Count > 0; }
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

        public ICommand AddNotificationClicked
        {
            get
            {
                return new Command((item) =>
                {
                    m_items.Add(string.Format("Notification {0}", m_items.Count + 1));
                    PropertyChanged.Fire(this, "StatusStrings");
                    PropertyChanged.Fire(this, "HasNotifications");                    
                });
            }
        }
    }
}
