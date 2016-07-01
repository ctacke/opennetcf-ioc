using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OpenNETCF.IoC
{
    public static class IoCLocalDevice
    {
        private static string _rootPath;
        private static string _executingAssemblyFullPath;

        public static string GetPlatformPath(string s)
        {
            var path = s.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

            // not sure this is valid for all cases, we'll need to do heavy testing
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(RootPath, path);
            }

            return path;

        }
        
        public static string ExecutingAssemblyFullPath
        {
            get
            {

                if (string.IsNullOrEmpty(_executingAssemblyFullPath))
                {
                    Assembly getPathFrom = null;

#if !WindowsCE
                    getPathFrom = Assembly.GetEntryAssembly();
#endif

                    if (getPathFrom == null)
                    {
                        getPathFrom = Assembly.GetExecutingAssembly();
                    }
                    var uri = new Uri(getPathFrom.GetName().CodeBase);
                    _executingAssemblyFullPath = GetPlatformPath(uri.LocalPath);
                }
                return _executingAssemblyFullPath;
            }

            set
            {
                _executingAssemblyFullPath = value;
            }
        }

        public static string RootPath
        {
            get
            {

                if (string.IsNullOrEmpty(_rootPath))
                {
#if ANDROID
                   return Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
#endif

                    Assembly getPathFrom = null;

#if !WindowsCE
                    getPathFrom = Assembly.GetEntryAssembly();
#endif

                    if (getPathFrom == null)
                    {
                        getPathFrom = Assembly.GetExecutingAssembly();
                    }
                    var uri = new Uri(getPathFrom.GetName().CodeBase);
                    _rootPath = Path.GetDirectoryName(uri.LocalPath);
                }
                return _rootPath;
            }

            set
            {
                _rootPath = value;
            }
        }
    }
}
