using OpenNETCF.GA;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OpenNETCF.IoC
{
    public class ViewTypeNotRegisteredException : Exception
    {
        public ViewTypeNotRegisteredException(Type viewType)
            : base(string.Format("View type '{0}' not registered", viewType.Name))
        {
        }
    }

    public interface IView
    {
        object BindingContext { get; set; }
    }

    public interface IViewModel : INotifyPropertyChanged
    {
    }

    public class AnalyticsSettings
    {
        private static AnalyticsService m_analytics;
        private bool m_inSession;

        internal bool ScreenTrackingEnabled { get; private set; }
        internal bool TimingTrackingEnabled { get; private set; }

        public const string DefaultTimingCategory = "Navigation";

        public string TimingCategory { get; set; }

        internal AnalyticsSettings()
        {
            // did someone already register analytics? If so, we'll just use that
            var test = AnalyticsService;
            TimingCategory = DefaultTimingCategory;
        }

        private AnalyticsService AnalyticsService
        {
            get
            {
                if (m_analytics == null)
                {
                    var existing = RootWorkItem.Services.Get<AnalyticsService>();
                    if (existing != null)
                    {
                        m_analytics = existing;
                    }
                }

                return m_analytics;
            }
        }

        public void Register(string trackingID, string appName, Version appVersion = null)
        {
            m_analytics = new AnalyticsService(trackingID, applicationName: appName, applicationVersion: appVersion);
            RootWorkItem.Services.Add(m_analytics);
        }

        public void EnableScreenTracking()
        {
            if (m_analytics == null) throw new Exception("Analytics not initialized.");
            ScreenTrackingEnabled = true;
        }

        public void DisableScreenTracking()
        {
            if (m_analytics == null) throw new Exception("Analytics not initialized.");
            ScreenTrackingEnabled = false;
        }

        public void EnableTimingTracking()
        {
            if (m_analytics == null) throw new Exception("Analytics not initialized.");
            TimingTrackingEnabled = true;
        }

        public void DisableTimingTracking()
        {
            if (m_analytics == null) throw new Exception("Analytics not initialized.");
            TimingTrackingEnabled = false;
        }

        public void StartTrackingSession()
        {
            if (m_analytics == null) throw new Exception("Analytics not initialized.");

            m_analytics.TrackSessionStart();
            m_inSession = true;
        }

        public void StopTrackingSession()
        {
            if (m_analytics == null) throw new Exception("Analytics not initialized.");

            m_analytics.TrackSessionEnd();
            m_inSession = false;
        }

        internal string GetPageName(Page page)
        {
            if (page == null) return null;

            var name = page.Title;
            if (name.IsNullOrEmpty())
            {
                name = page.GetType().Name;
            }

            // TODO: implement user-overridable name lookups

            return name;
        }

        private int m_lastPageShownTick;

        internal void LogPageNavigation(string fromPage, string toPage)
        {
            if (!ScreenTrackingEnabled) return;

            if (m_analytics == null) throw new Exception("Analytics not initialized.");

            m_analytics.TrackScreenView(toPage);

            var now = Environment.TickCount;
            if (!fromPage.IsNullOrEmpty())
            {
                if (m_lastPageShownTick != 0)
                {
                    if (TimingTrackingEnabled)
                    {
                        var delta = now - m_lastPageShownTick;
                        AnalyticsService.TrackTiming(TimingCategory, fromPage, delta);
                    }
                }
            }
            m_lastPageShownTick = now;
        }
    }

    public static class NavigationService
    {
        // this is a viewType : viewModelType lookup table
        private static SafeDictionary<Type, Type> m_index = new SafeDictionary<Type, Type>();
        private static Page m_mainView;

        public static AnalyticsSettings Analytics { get; private set; } = new AnalyticsSettings();

        public static Page CurrentView
        {
            get
            {
                if (m_mainView == null) return null;
                if (m_mainView.Navigation.NavigationStack.Count == 0) return null;

                var page = m_mainView.Navigation.NavigationStack.Last();
                if (page is CarouselPage)
                {
                    return (page as CarouselPage).CurrentPage;
                }
                return page;
            }
        }

        public static void SetMainView<TView>(bool wrapInNavigationPage)
            where TView : Page
        {
            var fromPageName = Analytics.GetPageName(CurrentView);

            var view = CreateViewAndViewModel<TView>();
            if (wrapInNavigationPage)
            {
                m_mainView = new NavigationPage(view)
                {
                    Title = view.Title
                };

            }
            else
            {
                m_mainView = view;
            }
            
            Application.Current.MainPage = m_mainView;

            var toPageName = Analytics.GetPageName(m_mainView);
            Analytics.LogPageNavigation(fromPageName, toPageName);
        }

        public static void Register<TView, TViewModel>()
            where TView : Page
            where TViewModel : class, IViewModel, new()
        {
            lock (m_index)
            {
                var viewType = typeof(TView);
                var viewModelType = typeof(TViewModel);
                if (!m_index.ContainsKey(viewType))
                {
                    m_index.Add(viewType, viewModelType);
                }
            }
        }

        public async static void ShowHome()
        {
            var fromPageName = Analytics.GetPageName(CurrentView);

            await m_mainView.Navigation.PopToRootAsync(true);

            var toPageName = Analytics.GetPageName(CurrentView);
            Analytics.LogPageNavigation(fromPageName, toPageName);
        }

        public async static void NavigateForward<TView>()
            where TView : Page
        {
            await ShowView<TView>(true, false);
        }

        public async static void NavigateForward<TView>(bool animated)
            where TView : Page
        {
            await ShowView<TView>(animated, false);
        }

        public async static void ShowModal<TView>(bool animated)
            where TView : Page
        {
            await ShowView<TView>(animated, true);
        }

        public async static void HideModal(bool animated)
        {
            var fromPageName = Analytics.GetPageName(CurrentView);

            await m_mainView.Navigation.PopModalAsync(animated);

            var toPageName = Analytics.GetPageName(CurrentView);
            Analytics.LogPageNavigation(fromPageName, toPageName);
        }

        public async static void NavigateBack(bool animated)
        {
            var fromPageName = Analytics.GetPageName(CurrentView);

            await m_mainView.Navigation.PopAsync(animated);

            var toPageName = Analytics.GetPageName(CurrentView);
            Analytics.LogPageNavigation(fromPageName, toPageName);
        }

        private async static Task ShowView<TView>(bool animated, bool modal)
            where TView : Page
        {            
            var view = CreateViewAndViewModel<TView>();

            var fromViewName = Analytics.GetPageName(CurrentView);
            var toViewName = Analytics.GetPageName(view);

            // now show it
            if (modal)
            {
                await m_mainView.Navigation.PushModalAsync(view, animated);
            }
            else
            {
                await m_mainView.Navigation.PushAsync(view, animated);
            }

            Analytics.LogPageNavigation(fromViewName, toViewName);
        }

        private static TView CreateViewAndViewModel<TView>()
            where TView : Page
        {
            var viewType = typeof(TView);
            Type viewModelType = null;

            lock (m_index)
            {
                if (!m_index.ContainsKey(viewType))
                {
                    throw new ViewTypeNotRegisteredException(viewType);
                }
                viewModelType = m_index[viewType];
            }
            // do we have a view already created?
            var view = RootWorkItem.Services.Get<TView>();

            if (view == null)
            {
                try
                {
                    view = RootWorkItem.Services.AddNew<TView>();
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to create View: " + viewType.Name, ex);
                }
            }

            // do we have a viewmodel already created?
            var viewModel = RootWorkItem.Services.Get(viewModelType) as IViewModel;

            if (viewModel == null)
            {
                try
                {
                    viewModel = RootWorkItem.Services.AddNew(viewModelType) as IViewModel;
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to create ViewModel: " + viewModelType.Name, ex);
                }
            }

            // make sure the binding is set
            view.BindingContext = viewModel;

            return view;
        }

        public static TViewModel GetViewModel<TViewModel>()
            where TViewModel : class, IViewModel
        {
            var viewModelType = typeof(TViewModel);
            var viewModel = RootWorkItem.Services.Get(viewModelType) as TViewModel;

            if (viewModel == null)
            {
                try
                {
                    viewModel = RootWorkItem.Services.AddNew(viewModelType) as TViewModel;
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to create ViewModel: " + viewModelType.Name, ex);
                }
            }

            return viewModel;
        }

        public static IViewModel GetViewModelForView<TView>()
            where TView : Page
        {
            var viewType = typeof(TView);
            Type viewModelType = null;

            lock (m_index)
            {
                if (!m_index.ContainsKey(viewType))
                {
                    throw new ViewTypeNotRegisteredException(viewType);
                }
                viewModelType = m_index[viewType];
            }
            var existing = RootWorkItem.Services.Get(viewModelType) as IViewModel;

            if (existing == null)
            {
                CreateViewAndViewModel<TView>();
            }

            return RootWorkItem.Services.Get(viewModelType) as IViewModel;
        }

        public static Page GetViewForViewModel<TViewModel>()
            where TViewModel : IViewModel
        {
            var viewModelType = typeof(TViewModel);

            lock (m_index)
            {
                var tuple = m_index.FirstOrDefault(v => v.Value == viewModelType);

                if (tuple.Equals(default(KeyValuePair<Type, Type>))) return null;  // not registered - not sure this is a valid compare.  Maybe should check the items in the tuple?

                return RootWorkItem.Services.Get(tuple.Key) as Page;
            }
        }

        public static Page GetView<TView>()
            where TView : Page
        {
            return CreateViewAndViewModel<TView>();
        }
    }
}