// LICENSE
// -------
// This software was originally authored by Christopher Tacke of OpenNETCF Consulting, LLC
// On March 10, 2009 is was placed in the public domain, meaning that all copyright has been disclaimed.
//
// You may use this code for any purpose, commercial or non-commercial, free or proprietary with no legal 
// obligation to acknowledge the use, copying or modification of the source.
//
// OpenNETCF will maintain an "official" version of this software at www.opennetcf.com and public 
// submissions of changes, fixes or updates are welcomed but not required
//

#if WINDOWS_PHONE || ANDROID || CF_20
using Trace = System.Diagnostics.Debug;
#endif

#if DESKTOP
using System.Linq;
#endif

using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Reflection;

namespace OpenNETCF.IoC.UI
{
    public abstract class SmartClientApplication<TShell> : SmartClientApplication
        where TShell : Form
    {
        public Form ShellForm { get; private set; }

        /// <summary>
        /// If <b>true</b>, only one instance of the application will run.  Subsequent attempts will activate the existing app
        /// </summary>
        public virtual bool IsSingletonApp
        {
            get { return true; }
        }

        public virtual bool EnableShellReplacement
        {
            get { return true; }
        }

#if DESKTOP
        private Mutex m_singletonMutex;
        private const int SW_RESTORE = 9;

        private bool HandleSingleton()
        {
            var mutexName = Assembly.GetEntryAssembly().GetName().Name + "_mutex";

            bool createdNew;
            m_singletonMutex = new Mutex(true, mutexName, out createdNew);
            if (createdNew)
            {
                //// first instance
                return false;
            }
            else
            {
                // subsequent instance
                var name = Assembly.GetEntryAssembly().GetName().Name;
                var p = Process.GetProcessesByName(name).FirstOrDefault();
                if (p != null)
                {
                    NativeMethods.SetForegroundWindow(p.MainWindowHandle);
                }

                // TODO: pass parameters to existing instance

                return true;
            }
        }
#endif

        public override void Start(IModuleInfoStore store)
        {

#if DESKTOP
            if (IsSingletonApp)
            {
                if (HandleSingleton()) return;
            }
#endif

            // add a generic "control" to the Items list.
            var invoker = new Control();
            // force handle creation
            var handle = invoker.Handle;
            RootWorkItem.Items.Add(invoker, Constants.EventInvokerName);
            ModuleInfoStoreService storeService = RootWorkItem.Services.AddNew<ModuleInfoStoreService>();

            AddServices();

            if (store != null)
            {
                storeService.ModuleLoaded += new EventHandler<GenericEventArgs<IModuleInfo>>(OnModuleLoaded);
                storeService.LoadModulesFromStore(store);
            }

            BeforeShellCreated();

            // create the shell form after all modules are loaded
            // see if there's a registered shell replacement.
            ShellReplacement replacement = null;

            if (EnableShellReplacement)
            {
                replacement = RootWorkItem.Services.Get<ShellReplacement>();
            }

            if ((replacement == null) || (!replacement.ShellReplacementEnabled))
            {
                ShellForm = RootWorkItem.Items.AddNew<TShell>();
            }
            else
            {
                ShellForm = replacement as Form;
                Trace.WriteLine("Replacement shell found.", Constants.TraceCategoryName);
            }

            AfterShellCreated();

            OnApplicationRun(ShellForm);

            OnApplicationClose();
        }

        public virtual Type ShellFormType
        {
            get { return typeof(TShell); }
        }

        protected virtual void AfterShellCreated()
        {
        }

        protected virtual void BeforeShellCreated()
        {
        }

        protected virtual void OnApplicationClose()
        {
            try
            {
                ShellForm.Dispose();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ShellForm.Dispose threw: ", ex.Message);
            }
        }

        /// <summary>
        /// When overridden by a derived class, an application can choose to use an alternate for Application.Run.
        /// </summary>
        /// <remarks>
        /// The Compact Framework does not support IMessageFilter, so if you want to add one you must use something like OpenNETCF's Application2
        /// class.  By overriding this method, you can create and add filters and then call Application2.Run.  Note that you <b>must</b> call some form of Run
        /// method (Application.Run or otherwise) if you override this.  It's also ill advised to call the base implementation if you use your own Run
        /// implementation.
        /// </remarks>
        /// <param name="form">Form instance to be passed tot he Run method.</param>
        public virtual void OnApplicationRun(Form form)
        {
            Application.Run(form);
        }
    }
}
