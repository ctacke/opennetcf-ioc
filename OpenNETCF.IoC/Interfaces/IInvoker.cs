using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenNETCF.IoC
{
    public interface IInvoker
    {
        MethodInfo HandlerMethod { get; }
        void Initialize(object invokerObject, Delegate targetDelegate);
        object CreateInvokerObject();
    }
}
