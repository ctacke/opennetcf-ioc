using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace OpenNETCF.Controls
{
    public partial class StatusBar : StackLayout
    {
        public static readonly BindableProperty StatusItemsProperty =
            BindableProperty.Create("StatusItems", typeof(object[]), typeof(StatusBar), defaultBindingMode: BindingMode.TwoWay);

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create("Command", typeof(ICommand), typeof(StatusBar));

        private Color m_backColor;
        private int m_scrollPeriod;
        private int m_currentIndex;
        private bool m_skipNextScroll = false;

        public StatusBar()
        {
            InitializeComponent();

            this.BackgroundColor = Color.Red;
            this.TextColor = Color.White;

            AutoscrollPeriod = 5;

            StatusIcon.Source = ImageSource.FromResource("OpenNETCF.Resources.error_64px.png");
            PreviousIcon.Source = ImageSource.FromResource("OpenNETCF.Resources.chevron_left_round_64px.png");
            NextIcon.Source = ImageSource.FromResource("OpenNETCF.Resources.chevron_right_round_64px.png");

            var nextRecognizer = new TapGestureRecognizer();
            nextRecognizer.Tapped += NextRecognizer_Tapped;
            NextIcon.GestureRecognizers.Add(nextRecognizer);

            var previousRecognizer = new TapGestureRecognizer();
            previousRecognizer.Tapped += PreviousRecognizer_Tapped;
            PreviousIcon.GestureRecognizers.Add(previousRecognizer);

            var statusRecognizer = new TapGestureRecognizer();
            statusRecognizer.Tapped += StatusRecognizer_Tapped;
            CountLabel.GestureRecognizers.Add(statusRecognizer);
            StatusLabel.GestureRecognizers.Add(statusRecognizer);
        }

        private void StatusRecognizer_Tapped(object sender, EventArgs e)
        {
            if (Command != null && Command.CanExecute(null))
            {
                Command.Execute(CurrentObject);
            }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == "StatusItems")
            {
                SetValue(StatusItemsProperty, StatusItems);
                m_currentIndex = 0;
                UpdateStatusText();
            }
        }

        private void PreviousRecognizer_Tapped(object sender, EventArgs e)
        {
            m_skipNextScroll = true;
            DoScrollPrevious();
        }

        private void NextRecognizer_Tapped(object sender, EventArgs e)
        {
            m_skipNextScroll = true;
            DoScrollNext();
        }

        public int AutoscrollPeriod
        {
            get { return m_scrollPeriod; }
            set
            {
                var tmp = value;
                if (tmp < 0) tmp = 0;

                // if we were at zero, we need to start
                if (AutoscrollPeriod == 0)
                {
                    m_scrollPeriod = tmp;
                    Task.Run(async () => { await Scroll(); });
                }

                m_scrollPeriod = tmp;
            }
        }

        private async Task Scroll()
        {
            while(AutoscrollPeriod > 0)
            {
                await Task.Delay(AutoscrollPeriod * 1000);

                if (!m_skipNextScroll)
                {
                    DoScrollNext();
                }
                m_skipNextScroll = false;
            }
        }

        private void UpdateStatusText()
        {
            if (StatusItems == null) return;

            Device.BeginInvokeOnMainThread(() =>
                    {
                        if (StatusItems.Length == 0)
                        {
                            CountLabel.Text = string.Empty;
                            StatusLabel.Text = string.Empty;

                            CountLabel.IsVisible = false;
                            PreviousIcon.IsVisible = false;
                            NextIcon.IsVisible = false;
                        }
                        else if (StatusItems.Length == 1)
                        {
                            CountLabel.IsVisible = false;
                            PreviousIcon.IsVisible = false;
                            NextIcon.IsVisible = false;

                            StatusLabel.Text = StatusItems[m_currentIndex].ToString();
                        }
                        else
                        {
                            CountLabel.IsVisible = true;
                            PreviousIcon.IsVisible = true;
                            NextIcon.IsVisible = true;

                            CountLabel.Text = string.Format("({0} of {1})", m_currentIndex + 1, StatusItems.Length);
                            StatusLabel.Text = StatusItems[m_currentIndex].ToString();
                        }
                    });
        }

        private void DoScrollNext()
        {
            if ((StatusItems != null) && (StatusItems.Length > 0))
            {
                m_currentIndex++;
                if (m_currentIndex >= StatusItems.Length) m_currentIndex = 0;

                UpdateStatusText();
            }
        }

        private void DoScrollPrevious()
        {
            if ((StatusItems != null) && (StatusItems.Length > 0))
            {
                m_currentIndex--;
                if (m_currentIndex < 0) m_currentIndex = StatusItems.Length - 1;

                UpdateStatusText();
            }
        }

        public Color TextColor
        {
            get { return StatusLabel.TextColor; }
            set { CountLabel.TextColor = StatusLabel.TextColor = value; }
        }

        public string Text
        {
            get { return StatusLabel.Text; }
            set { StatusLabel.Text = value; }
        }

        public new Color BackgroundColor
        {
            get { return m_backColor; }
            set
            {
                m_backColor = value;
                base.BackgroundColor = value;

                StatusIcon.BackgroundColor = this.BackgroundColor;
                PreviousIcon.BackgroundColor = this.BackgroundColor;
                NextIcon.BackgroundColor = this.BackgroundColor;
                StatusLabel.BackgroundColor = this.BackgroundColor;
                CountLabel.BackgroundColor = this.BackgroundColor;
            }
        }

        public object[] StatusItems
        {
            get { return (object[])GetValue(StatusItemsProperty); }
            set { SetValue(StatusItemsProperty, value); }
        }

        public object CurrentObject
        {
            get
            {
                if (StatusItems == null) return null;
                if (StatusItems.Length == 0) return null;
                return StatusItems[m_currentIndex];
            }
        }
    }
}
