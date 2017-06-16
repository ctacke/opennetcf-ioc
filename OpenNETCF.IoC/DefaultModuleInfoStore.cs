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

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace OpenNETCF.IoC
{
    public class DefaultModuleInfoStore : IModuleInfoStore
    {
        private string m_catalogFilePath;

      
        public DefaultModuleInfoStore()
        {
            CatalogFilePath = Path.Combine(IoCLocalDevice.RootPath, Constants.DefaultProfileCatalogName);
        }

        public DefaultModuleInfoStore(string profilePath)
        {
            CatalogFilePath = Path.Combine(IoCLocalDevice.RootPath, profilePath);
        }

        public Assembly[] GetModuleAssemblies()
        {
            return null;
        }

        public string GetModuleListXml()
        {
            try
            {
                if (!File.Exists(m_catalogFilePath))
                {
                    return null;

                }
                using (TextReader reader = File.OpenText(m_catalogFilePath))
                {
                    return reader.ReadToEnd();
                }
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public string CatalogFilePath
        {
            get { return m_catalogFilePath; }
            set
            {
                Validate
                    .Begin()
                    .IsNotNullOrEmpty(value)
                    .Check();

                m_catalogFilePath = value;
            }
        }
    }
}
