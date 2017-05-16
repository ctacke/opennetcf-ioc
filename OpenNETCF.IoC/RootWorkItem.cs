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

namespace OpenNETCF.IoC
{
    public static class RootWorkItem
    {
        internal static WorkItem m_workItem;

        static RootWorkItem()
        {
            m_workItem = new WorkItem();
        }

        public static ManagedObjectCollection<WorkItem> WorkItems
        {
            get { return m_workItem.WorkItems; }
        }

        public static ManagedObjectCollection<object> Items 
        {
            get { return m_workItem.Items; }
        }

        public static ServiceCollection Services
        {
            get { return m_workItem.Services; }
        }

#if (!NO_WINFORMS) && (!PCL)
        public static ManagedObjectCollection<UI.IWorkspace> Workspaces
        {
            get { return m_workItem.Workspaces; }
        }

        public static ManagedObjectCollection<UI.ISmartPart> SmartParts
        {
            get { return m_workItem.SmartParts; }
        }

        public static ModuleCollection Modules
        {
            get { return m_workItem.Modules; }
        }
#endif
        public static WorkItem Instance
        {
            get { return m_workItem; }
        }

        public static void RegisterType(Type concreteType, Type registerAs)
        {
            Instance.RegisterType(concreteType, registerAs);
        }

        public static void BeginInvoke(Delegate method)
        {
            method.DynamicInvoke(null);
        }

        public static void BeginInvoke(Delegate method, params object[] args)
        {
            method.DynamicInvoke(args);
        }

        public static void Invoke(Delegate method)
        {
            method.DynamicInvoke(null);
        }

        public static void Invoke(Delegate method, params object[] args)
        {
            method.DynamicInvoke(args);
        }
    }
}
