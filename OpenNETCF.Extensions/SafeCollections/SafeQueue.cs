using OpenNETCF;
using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace System.Collections.Generic
{
    public class SafeQueue<T> : IEnumerable<T>
    {
        public event EventHandler<GenericEventArgs<T>> ItemAdded;

        private Queue<T> m_queue;
        private readonly object m_syncRoot = new object();
        private AutoResetEvent m_are;

        public SafeQueue()
        {
            m_queue = new Queue<T>();
            m_are = new AutoResetEvent(false);
        }

        public IEnumerator<T> GetEnumerator()
        {
            // instead of returning an unsafe enumerator,
            // we wrap it into our thread-safe class
            lock (m_syncRoot)
            {
                return new SafeEnumerator<T>(m_queue.GetEnumerator(), m_syncRoot);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get 
            {
                lock (m_syncRoot)
                {
                    return m_queue.Count;
                }
            }
        }

        public void Enqueue(T item)
        {
            lock (m_syncRoot)
            {
                m_queue.Enqueue(item);
                m_are.Set();
            }
            ItemAdded.Fire(this, item);
        }

        public T Dequeue()
        {
            lock (m_syncRoot)
            {
                return m_queue.Dequeue();
            }
        }

        public T Peek()
        {
            lock (m_syncRoot)
            {
                return m_queue.Peek();
            }
        }

        public bool EnqueueWaitOne(int millisecondsTimeout)
        {
            return m_are.WaitOne(millisecondsTimeout);
        }
    }
}
