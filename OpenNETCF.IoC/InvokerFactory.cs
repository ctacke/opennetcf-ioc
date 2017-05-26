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
using System.IO;
using System.Linq;
using System.Reflection;

namespace OpenNETCF.IoC
{
    internal static class InvokerFactory
    {
        private static Assembly m_invokerAssembly = null;
        private static Type m_invokerType = null;

        private static Assembly InvokerAssembly
        {
            get
            {
                if (m_invokerAssembly == null)
                {
                    var assemblyName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Locati‌​on);

                    switch (Environment.OSVersion.Platform)
                    {
                        case PlatformID.Unix:
                        case PlatformID.MacOSX:
                            assemblyName = Path.Combine(assemblyName, "OpenNETCF.IoC.Invoker.Mono.dll");
                            break;
                        case PlatformID.WinCE:
                            throw new NotSupportedException();
                        case PlatformID.Xbox:
                            throw new NotSupportedException();
                        default:
                            assemblyName = Path.Combine(assemblyName, "OpenNETCF.IoC.Invoker.FFx.dll");
                            break;
                    }
                    m_invokerAssembly = Assembly.LoadFile(assemblyName);
                }

                return m_invokerAssembly;
            }
        }

        private static Type InvokerType
        {
            get
            {
                if (m_invokerType == null)
                {
                    var interfaceType = typeof(IInvoker);

                    var type = InvokerAssembly.GetTypes()
                        .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                        .FirstOrDefault();

                    if (type == null)
                    {
                        throw new Exception("No IInvoker implementation found!");
                    }
                    m_invokerType = type;
                }
                return m_invokerType;
            }
        }

        public static IInvoker GetInvoker(object invoker, Delegate targetDelegate)
        {
            var instance = (IInvoker)Activator.CreateInstance(InvokerType);
            instance.Initialize(invoker, targetDelegate);

            return instance;
        }

        public static object GetInvokerObject()
        {
            var instance = (IInvoker)Activator.CreateInstance(InvokerType);
            return instance.CreateInvokerObject();
        }
    }
}
