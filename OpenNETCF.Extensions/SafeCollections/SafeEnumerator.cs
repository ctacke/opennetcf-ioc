using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace System
{
    public class SafeEnumerator<T> : IEnumerator<T>
    {
        // this is the (thread-unsafe)
        // enumerator of the underlying collection
        private readonly IEnumerator<T> m_inner;
        // this is the object we shall lock on. 
        private readonly object m_lock;

        public SafeEnumerator(IEnumerator<T> inner, object @lock)
        {
            m_inner = inner;
            m_lock = @lock;

            // entering lock in constructor
#if(WindowsCE)
            if (!Monitor.TryEnter(m_lock))
#else
            if(!Monitor.TryEnter(m_lock, 100000)) 
#endif
            {
                if (Debugger.IsAttached)  Debugger.Break();

                throw new Exception("Unable to lock enumerator");
            }
        }

        public void Dispose()
        {
            // .. and exiting lock on Dispose()
            // This will be called when foreach loop finishes
            m_inner.Dispose();
            Monitor.Exit(m_lock);
        }

        // we just delegate actual implementation
        // to the inner enumerator, that actually iterates
        // over some collection

        public bool MoveNext()
        {
            return m_inner.MoveNext();
        }

        public void Reset()
        {
            m_inner.Reset();
        }

        public T Current
        {
            get { return m_inner.Current; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}
