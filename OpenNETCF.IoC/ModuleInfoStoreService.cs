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

#if !CF_20
using System.Xml.Linq;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace OpenNETCF.IoC
{
    public sealed class ModuleInfoStoreService
    {
        public event EventHandler<GenericEventArgs<IModuleInfo>> ModuleLoaded;

        private List<IModuleInfo> m_loadedModules = new List<IModuleInfo>();
        private WorkItem m_root;
        private object m_syncRoot = new object();

        public ModuleInfoStoreService()
            : this(RootWorkItem.Instance)
        {
        }

        internal ModuleInfoStoreService(WorkItem root)
        {
            m_root = root;
        }

        public void LoadModulesFromStore(IModuleInfoStore store)
        {
            Validate
                .Begin()
                .IsNotNull(store, "store")
                .Check();

            // load modules from XML
            string xml = store.GetModuleListXml();
            if (xml != null)
            {

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ConformanceLevel = ConformanceLevel.Document;
                settings.IgnoreWhitespace = true;
                settings.IgnoreComments = true;

                var assemblyNames = GetAssembliesFromXml(xml);

                // load each assembly
                LoadAssemblies(assemblyNames);
            }

            // now let the IModuleInfoStore directly load assemblies and pass them back
            var assemblies = store.GetModuleAssemblies();

            if(assemblies != null)
            {
                foreach (var assembly in assemblies)
                {
                    LoadAssembly(assembly);
                }
            }

            // now notify all assemblies that all other assemblies are loaded (this is useful when there are module interdependencies
            NotifyAssembliesOfContainerCompletion();
        }

#if CF_20
        private string[] GetAssembliesFromXml(string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var modules = new List<string>();

            foreach (XmlNode mi in doc.SelectNodes("/SolutionProfile/Modules/ModuleInfo"))
            {
                modules.Add(mi.Attributes["AssemblyFile"].Value);
            }

            return modules.ToArray();
        }
#else
        private string[] GetAssembliesFromXml(string xml)
        {
            var root = XElement.Parse(xml);

            string s = "Modules";
            var modules = from n in root.Descendants(s)
                          select n;

            return (from n in modules.Descendants()
                    where n.Name == "ModuleInfo"
                    select n.Attribute("AssemblyFile").Value).ToArray();
        }
#endif

        private void NotifyAssembliesOfContainerCompletion()
        {
            lock (m_syncRoot)
            {
            foreach (var m in m_loadedModules)
            {
                var loadComplete = ((ModuleInfo)m).Instance.GetType().GetMethod("OnContainerLoadComplete", BindingFlags.Public | BindingFlags.Instance);
                if (loadComplete != null)
                {
                    try
                    {
                        loadComplete.Invoke(((ModuleInfo)m).Instance, null);
                    }
                    catch (Exception ex)
                    {
                        throw ex.InnerException;
                    }
                }
            }
        }
        }

        private Type FindIModuleType(Assembly assembly)
        {
            Type imodule;

            // see if we have an explicitly defined entry
            var attrib = (from a in assembly.GetCustomAttributes(true)
                          where a is IoCModuleEntryAttribute
                          select a as IoCModuleEntryAttribute).FirstOrDefault();

            if (attrib != null)
            {
                if (!attrib.EntryType.Implements<IModule>())
                {
                    throw new Exception(
                        string.Format("IoCModuleEntry.EntryType in assembly '{0}' doesn't derive from IModule",
                        assembly.FullName));
                }

                imodule = attrib.EntryType;
            }
            else
            {
                // default to old behavior - loading will be *much* slower under Mono as we have to call GetTypes()
                // under CF and FFX this appears negligible
                try
                {
                    imodule = (from t in assembly.GetTypes()
                               where t.GetInterfaces().Count(i => i.Equals(typeof(IModule))) > 0
                               select t).FirstOrDefault(m => !m.IsAbstract);
                }
#if !WindowsCE
                catch (ReflectionTypeLoadException ex)
                {
                    Trace.WriteLine(string.Format("IoC: Exception loading assembly '{0}': {1}", assembly.FullName, ex.Message), Constants.TraceCategoryName);

                    throw;
                }
#else
                catch(Exception ex)
                {
                    Trace.WriteLine(string.Format("IoC: Exception loading assembly '{0}': {1}", assembly.FullName, ex.Message), Constants.TraceCategoryName);

                    throw;
                }
#endif
            }

            return imodule;
        }

        internal ModuleInfo LoadAssembly(Assembly assembly)
        {
            var assemblyName = assembly.GetName();
            Trace.WriteLine(string.Format("IoC: Loading assembly '{0}'", assemblyName), Constants.TraceCategoryName);

            Type imodule = FindIModuleType(assembly);
            if (imodule == null) return null;

            object instance = ObjectFactory.CreateObject(imodule, RootWorkItem.Instance);

            var info = new ModuleInfo
                {
                    Assembly = assembly,
                    AssemblyFile = assemblyName.CodeBase,
                    Instance = instance
                };

            lock (m_syncRoot)
            {
                m_loadedModules.Add(info);
            }

            var loadMethod = imodule.GetMethod("Load", BindingFlags.Public | BindingFlags.Instance);
            if (loadMethod != null)
            {
                try
                {
                    loadMethod.Invoke(instance, null);
                }
                catch (Exception ex)
                {
                    throw ex.InnerException;
                }
            }

            var addServices = imodule.GetMethod("AddServices", BindingFlags.Public | BindingFlags.Instance);
            if (addServices != null)
            {
                try
                {
                    addServices.Invoke(instance, null);
                }
                catch (Exception ex)
                {
                    throw ex.InnerException;
                }
            }

            ModuleLoaded.Fire(this, new GenericEventArgs<IModuleInfo>(info));

            return info;
        }

        private void LoadAssemblies(IEnumerable<string> assemblyNames)
        {
            Validate
                .Begin()
                .IsNotNull(assemblyNames, "assemblyNames")
                .Check();

            Assembly asm = null;

            foreach (var s in assemblyNames)
            {
                var tryByPath = true;

                // avoid excepting by default under the Compact Framework
                //if (Environment.OSVersion.Platform != PlatformID.WinCE)
                //{
                //    try
                //    {
                //        if (File.Exists(s))
                //        {
                //            asm = Assembly.Load(s);
                //            tryByPath = false;
                //        }
                //    }
                //    catch (FileNotFoundException)
                //    {
                //        // this will try by path below
                //    }
                //}

                if (tryByPath)
                {
                    var rootFolder = Path.Combine(IoCLocalDevice.RootPath,s);

                    asm = null;

                    var fi = new FileInfo(rootFolder);

                    if (fi.Exists)
                    {
                        // local?
                        asm = Assembly.LoadFrom(fi.FullName);
                    }
                    else if (File.Exists(s))
                    {
                        // fully qualified path?
                        asm = Assembly.LoadFrom(s);
                    }
                    else if (Environment.OSVersion.Platform != PlatformID.Unix  && File.Exists(Path.Combine("\\Windows", s)))
                    {
                        // Windows?
                        asm = Assembly.LoadFrom(Path.Combine("\\Windows", s));
                    }
                    else
                    {
                        throw new IOException(string.Format("Unable to locate assembly '{0}'", s));
                    }
                }

                if (asm == null) continue;

                try
                {
                    LoadAssembly(asm);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("Exception loading assembly '{0}': {1}", asm.FullName, ex.Message));
                }
            }
        }

        public IEnumerable<IModuleInfo> LoadedModules
        {
            get
            {
                lock (m_syncRoot)
                {
                    return m_loadedModules.ToArray();
                }
            }
        }
    }
}
