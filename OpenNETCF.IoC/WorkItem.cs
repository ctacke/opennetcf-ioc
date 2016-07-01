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

using System;
using System.Threading;
using OpenNETCF.IoC.UI;

namespace OpenNETCF.IoC
{
    public class WorkItem : IDisposable
    {
        private ModuleCollection m_modules = null;

        public WorkItem()
        {
            WorkItems = new ManagedObjectCollection<WorkItem>(this);
            Items = new ManagedObjectCollection<object>(this);
            SmartParts = new ManagedObjectCollection<ISmartPart>(this);
            Workspaces = new ManagedObjectCollection<IWorkspace>(this);
            Services = new ServiceCollection(this);

            ThreadPool.QueueUserWorkItem(delegate
            {
                OnBuiltUp();
            });
        }

        public virtual void OnBuiltUp() { }

        public WorkItem Parent { get; internal set; }

        /// <summary>
        /// The Items collection contains Components unique by string ID.  Multiple Components of the same type can be added
        /// </summary>
        public ManagedObjectCollection<object> Items { get; private set; }

        /// <summary>
        /// The Items collection contains Components unique by string ID.  Multiple Components of the same type can be added
        /// </summary>
        public ManagedObjectCollection<WorkItem> WorkItems { get; private set; }

        /// <summary>
        /// The Services collection contains Components unique by registered type.  Only one Service of a given registering type can exist in the collection.
        /// </summary>
        public ServiceCollection Services { get; private set; }

        /// <summary>
        /// The SmartParts collection contains ISmartParts unique by string ID.  Multiple Components of the same type can be added
        /// </summary>
        public ManagedObjectCollection<ISmartPart> SmartParts { get; private set; }

        /// <summary>
        /// The Workspaces collection contains IWorkspaces unique by string ID.  Multiple Components of the same type can be added
        /// </summary>
        public ManagedObjectCollection<IWorkspace> Workspaces { get; private set; }

        /// <summary>
        /// The Modules collection contains IModules registered by Type
        /// </summary>
        public ModuleCollection Modules 
        {
            get
            {
                // lazy load to prevent startup perf killer
                if (m_modules == null)
                {
                    m_modules = new ModuleCollection(this);
                    m_modules.LoadModules();
                }

                return m_modules;
            }
        }

        private bool m_disposed = false;

        /// <summary>
        /// Sets the concrete type registration for all contained ManagedObjectCollections
        /// </summary>
        /// <param name="concreteType"></param>
        /// <param name="registerAs"></param>
        public void RegisterType(Type concreteType, Type registerAs)
        {
            WorkItems.RegisterType(concreteType, registerAs);
            Items.RegisterType(concreteType, registerAs);
            SmartParts.RegisterType(concreteType, registerAs);
            Workspaces.RegisterType(concreteType, registerAs);
            Services.RegisterType(concreteType, registerAs);
        }

        /// <summary>
        /// Sets the concrete type registration for all contained ManagedObjectCollections
        /// </summary>
        /// <param name="concreteType"></param>
        /// <param name="registerAs"></param>
        public void RegisterType<TConcrete, TRegisterAs>()
        {
            WorkItems.RegisterType<TConcrete, TRegisterAs>();
            Items.RegisterType<TConcrete, TRegisterAs>();
            SmartParts.RegisterType<TConcrete, TRegisterAs>();
            Workspaces.RegisterType<TConcrete, TRegisterAs>();
            Services.RegisterType<TConcrete, TRegisterAs>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    // TODO: remove any managed items if we end up creating them
                }

                m_disposed = true;
            }
        }

        /// <summary>
        /// The root WorkItem that contains this WorkItem
        /// </summary>
        public WorkItem RootWorkItem
        {
            get
            {
                WorkItem root = this;

                while(root.Parent != null)
                {
                    root = root.Parent;
                }

                return root;
            }
        }
    }
}
