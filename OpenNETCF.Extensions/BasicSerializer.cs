using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Linq;

namespace System.Runtime.Serialization
{
    public abstract class BasicSerializer<T>
    {
        public abstract T Serialize(object instance, bool includeNonPublicProperties);
        public abstract TOuptutType Deserialize<TOuptutType>(T data);

        public T Serialize(object source)
        {
            return Serialize(source, false);
        }

        protected virtual bool ShouldSerialize(PropertyInfo pi)
        {
            return true;
        }

        protected T OnSerialize(PropertyInfo property, object instance)
        {
            return default(T);
        }
    }
}
