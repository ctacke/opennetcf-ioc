// -------------------------------------------------------------------------------------------------------
// LICENSE INFORMATION
//
// - This software is licensed under the MIT shared source license.
// - The "official" source code for this project is maintained at http://oncfext.codeplex.com
//
// Copyright (c) 2010 OpenNETCF Consulting
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
// associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial 
// portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT 
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
// -------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

#if !PCL

namespace System
{
    public static class TypeCache
    {
        private static Dictionary<string, Type> m_cache;

        static TypeCache()
        {
            m_cache = new Dictionary<string, Type>();
        }

        public static bool TryFindType(string typeName, out Type t)
        {
            t = null;

            lock (m_cache)
            {
                if (m_cache.ContainsKey(typeName))
                {
                    t = m_cache[typeName];
                    return true;
                }

                Assembly[] assemblies;

#if(WindowsCE)
                assemblies = new Assembly[] { Assembly.GetExecutingAssembly() };
#else
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
#endif

                foreach (Assembly a in assemblies)
                {
                    t = a.GetType(typeName);
                    if (t != null)
                    {
                        m_cache.Add(typeName, t);
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public static class TypeExtensions
    {
        private static Dictionary<Type, ConstructorInfo> m_ctorCache = new Dictionary<Type, ConstructorInfo>();

        public static bool Implements<TInterface>(this Type baseType)
        {
            if (!(typeof(TInterface).IsInterface))
            {
                throw new ArgumentException("TInterface must be an interface type.");
            }

            return baseType.GetInterfaces().Contains(typeof(TInterface));
        }

        public static bool Implements(this Type instanceType, Type interfaceType)
        {
            if (!(interfaceType.IsInterface))
            {
                throw new ArgumentException("interfaceType must be an interface type.");
            }

            return instanceType.GetInterfaces().Contains(interfaceType);
        }

        public static ConstructorInfo GetDefaultConstructor(this Type objectType, bool includePrivateConstructor)
        {
            if (m_ctorCache.ContainsKey(objectType))
            {
                return m_ctorCache[objectType];
            }

            var flags = BindingFlags.Public | BindingFlags.Instance;
            if (includePrivateConstructor)
            {
                flags |= BindingFlags.NonPublic;
            }

            var ctor = objectType.GetConstructor(flags, null, new Type[0], null);
            m_ctorCache.Add(objectType, ctor);
            return ctor;
        }

        public static bool IsNullable(this Type objectType)
        {
            if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return true;
            }

            return false;
        }

        public static object[] GetCustomAttributes<T>(this Type type, bool inherit, bool searchInterfaces)
        {
            if (!searchInterfaces)
            {
                return type.GetCustomAttributes(inherit);
            }

            var attributeType = typeof(T);

            return type.GetCustomAttributes(attributeType, inherit)
                .Union(type.GetInterfaces()
                .SelectMany(interfaceType => interfaceType.GetCustomAttributes(attributeType, true)))
                .Distinct()
                .ToArray();
        }
    }
}
#endif