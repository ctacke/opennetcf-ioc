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
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Threading;

namespace OpenNETCF.IoC
{
    internal class ObjectFactory
    {
        internal static string GenerateServiceName(Type t)
        {
            return t.Name + "Service";
        }

        internal static string GenerateItemName<TItem>(Type t, ManagedObjectCollection<TItem> parent)
            where TItem : class
        {
            string name = string.Empty;
            int i = 0;
            do
            {
                name = t.Name + (++i).ToString();
            } while (parent[name] != null);
            return name;
        }

        internal static string GenerateItemName(Type t, WorkItem root)
        {
            string name = string.Empty;
            int i = 0;
            do
            {
                name = t.Name + (++i).ToString();
            } while (root.Items[name] != null);
            return name;
        }

        internal static object CreateObject(Type t, WorkItem root)
        {
            return Activator.CreateInstance(t, null);
        }
    }
}
