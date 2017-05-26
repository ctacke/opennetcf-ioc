using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNETCF.IoC
{
    public abstract class SmartClientApplication : DisposableBase
    {
        /// <summary>
        /// This method loads the Profile Catalog Modules by calling GetModuleInfoStore which, unless overridden, uses a DefaultModuleInfoStore instance.
        /// It then creates an instance of TShell and calls Application.Run with that instance.
        /// </summary>
        public void Start()
        {
            // load up the profile catalog here
            IModuleInfoStore store = GetModuleInfoStore();

            Start(store);
        }

        public void Start(string profileCatalog)
        {
            // load up the profile catalog here
            IModuleInfoStore store = new DefaultModuleInfoStore(profileCatalog);

            Start(store);
        }

        public virtual void Start(IModuleInfoStore store)
        {
            var invoker = InvokerFactory.GetInvokerObject();
            RootWorkItem.Items.Add(invoker, Constants.EventInvokerName);

            ModuleInfoStoreService storeService = RootWorkItem.Services.AddNew<ModuleInfoStoreService>();

            AddServices();

            if (store != null)
            {
                storeService.ModuleLoaded += new EventHandler<GenericEventArgs<IModuleInfo>>(OnModuleLoaded);
                storeService.LoadModulesFromStore(store);
            }

            OnApplicationRun();
        }

        public virtual IModuleInfoStore GetModuleInfoStore()
        {
            return new DefaultModuleInfoStore();
        }

        /// <summary>
        /// When overridden, allows an application to add Services to the RootWorkItem before modules are loaded.
        /// </summary>
        public virtual void AddServices()
        {
        }

        public void OnModuleLoaded(object sender, GenericEventArgs<IModuleInfo> e)
        {
            OnModuleLoadComplete(e.Value.Assembly.FullName);
        }

        public virtual void OnModuleLoadComplete(string moduleName)
        {
        }

        public virtual void OnApplicationRun()
        {
        }
    }
}
