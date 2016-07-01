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

namespace OpenNETCF.IoC
{
    public class WorkItem : IDisposable
    {
        public WorkItem()
        {
            WorkItems = new ManagedObjectCollection<WorkItem>(this);
            Items = new ManagedObjectCollection<object>(this);
            Services = new ServiceCollection(this);

            OnBuiltUp();
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
