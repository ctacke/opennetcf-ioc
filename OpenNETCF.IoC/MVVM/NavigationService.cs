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
    public static class NavigationService
    {
        // this is a viewType : viewModelType lookup table
        private static SafeDictionary<Type, Type> m_index = new SafeDictionary<Type, Type>();
        private static Page m_mainView;
        private static bool m_navigating;
        private static List<Type> m_multipagesBeingWatched = new List<Type>();

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
            where TView : Page, new()
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

        public static void HideNavigationBar()
        {
            if (m_mainView != null)
            {
                NavigationPage.SetHasNavigationBar(m_mainView, false);
            }
        }

        public static void ShowNavigationBar()
        {
            if (m_mainView != null)
            {
                NavigationPage.SetHasNavigationBar(m_mainView, true);
            }
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
            where TView : Page, new()
        {
            await ShowView<TView>(true, false);
        }

        public async static void NavigateForward<TView>(bool animated)
            where TView : Page, new()
        {
            await ShowView<TView>(animated, false);
        }

        public async static void ShowModal<TView>(bool animated)
            where TView : Page, new()
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

            if (m_navigating) return;
            try
            {
                await m_mainView.Navigation.PopAsync(animated);
            }
            finally
            {
                m_navigating = false;
            }

            var toPageName = Analytics.GetPageName(CurrentView);
            Analytics.LogPageNavigation(fromPageName, toPageName);
        }

        private async static Task ShowView<TView>(bool animated, bool modal)
            where TView : Page, new()
        {
            if (m_navigating) return;
            try
            {
                m_navigating = true;
                var view = CreateViewAndViewModel<TView>();

                var fromViewName = Analytics.GetPageName(CurrentView);
                var toViewName = Analytics.GetPageName(view);

                if (view.Parent != null)
                {
                    view.Parent = null;
                }

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
            catch (Exception ex)
            {
            }
            finally
            {
                m_navigating = false;
            }
        }

        private static TView CreateViewAndViewModel<TView>()
            where TView : Page, new()
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
                    // create the view
                    view = new TView();

                    // check to see if it's now registered
                    // if the View calls something like GetRegisteredViewModel, we'll get re-entered and it will already be registered
                    var existing = RootWorkItem.Services.Get<TView>();
                    if (existing == null)
                    {
                        RootWorkItem.Services.Add<TView>(view);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to create View: " + viewType.Name, ex);
                }
            }

            if (view is MultiPage<ContentPage>)
            {
                var t = typeof(View);
                if (!m_multipagesBeingWatched.Contains(t))
                {
                    (view as MultiPage<ContentPage>).CurrentPageChanged += OnMultiPageChanged;
                    m_multipagesBeingWatched.Add(t);
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

        private static void OnMultiPageChanged(object sender, EventArgs e)
        {
            var source = sender as CarouselPage;
            var toPage = Analytics.GetPageName(source.CurrentPage);
            Analytics.LogPageNavigation(null, toPage);
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
            where TView : Page, new()
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

        public static Page GetRegisteredView<TViewModel>(this TViewModel viewModel)
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

        public static IViewModel GetRegisteredViewModel<TView>(this TView view)
            where TView : Page, new()
        {
            var viewType = typeof(TView);
            Type viewModelType = null;

            // if the view hasn't been registered with the DI container, add it now
            if (!RootWorkItem.Services.Contains<TView>())
            {
                RootWorkItem.Services.Add(view);
            }

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

        public static TView GetView<TView>()
            where TView : Page, new()
        {
            return CreateViewAndViewModel<TView>();
        }
    }
}