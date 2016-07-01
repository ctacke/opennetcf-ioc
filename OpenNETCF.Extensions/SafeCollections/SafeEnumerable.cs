using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace System
{
    public class SafeEnumerable<T> : IEnumerable<T>
    {
        protected readonly IEnumerable<T> Inner;

        private readonly object m_Lock;

        public SafeEnumerable(IEnumerable<T> inner, object @lock)
        {
            m_Lock = @lock;
            Inner = inner;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new SafeEnumerator<T>(Inner.GetEnumerator(), m_Lock);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
