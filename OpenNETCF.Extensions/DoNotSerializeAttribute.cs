using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Linq;

namespace System.Runtime.Serialization
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DoNotSerializeAttribute : Attribute
    {
    }
}
