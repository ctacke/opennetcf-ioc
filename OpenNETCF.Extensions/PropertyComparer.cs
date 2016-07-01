using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

#if !XAMARIN

namespace OpenNETCF
{
    public class PropertyComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            return Equals(x, y, false);
        }

        public bool Equals(object x, object y, bool compareNonPublicProperties)
        {
            var type = x.GetType();

            if (!type.Equals(y.GetType())) return false;

            var flags = BindingFlags.Instance | BindingFlags.Public;
            if(compareNonPublicProperties)
            {
                flags |= BindingFlags.NonPublic;
            }
            var props = type.GetProperties(flags);
            foreach (var prop in props)
            {
                if (!prop.CanRead) continue;

                var a = prop.GetValue(x, null);
                var b = prop.GetValue(y, null);

                if (!a.Equals(b)) return false;
            }

            return true;
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}
#endif
