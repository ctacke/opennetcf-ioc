using OpenNETCF;
using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace System.Collections.Generic
{
    public class SafeList<T> : IList<T>, ICollection<T>, IEnumerable<T>
    {
        public event EventHandler<GenericEventArgs<T>> ItemAdded;

        private List<T> m_list;
        private readonly object m_syncRoot = new object();

        public SafeList()
        {
            m_list = new List<T>();
        }

        public SafeList(int capacity)
        {
            m_list = new List<T>(capacity);
        }

        public SafeList(IEnumerable<T> collection)
        {
            m_list = new List<T>(collection);
        }

        public IEnumerator<T> GetEnumerator()
        {
            // instead of returning an unsafe enumerator,
            // we wrap it into our thread-safe class
            return new SafeEnumerator<T>(m_list.GetEnumerator(), m_syncRoot);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            lock (m_syncRoot)
            {
                return m_list.IndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            lock (m_syncRoot)
            {
                m_list.Insert(index, item);
            }
            ItemAdded.Fire(this, item);
        }

        public void RemoveAt(int index)
        {
            lock (m_syncRoot)
            {
                m_list.RemoveAt(index);
            }
        }

        public T this[int index]
        {
            get
            {
                lock (m_syncRoot)
                {
                    return m_list[index];
                }
            }
            set
            {
                lock (m_syncRoot)
                {
                    m_list[index] = value;
                }
            }
        }

        public void Add(T item)
        {
            lock (m_syncRoot)
            {
                m_list.Add(item);
            }
            ItemAdded.Fire(this, item);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null) return;

            lock (m_syncRoot)
            {
                foreach (var item in collection)
                {
                    Add(item);
                }
            }
        }

        public void Clear()
        {
            lock (m_syncRoot)
            {
                m_list.Clear();
            }
        }

        public T Find(Predicate<T> match)
        {
            lock (m_syncRoot)
            {
                return m_list.Find(match);
            }
        }

        public bool Contains(T item)
        {
            lock (m_syncRoot)
            {
                return m_list.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (m_syncRoot)
            {
                m_list.CopyTo(array, arrayIndex);
            }
        }

        public int Count
        {
            get 
            {
                if( Monitor.TryEnter(m_syncRoot,5000))
                {
                    try
                    {
                        return m_list.Count;
                    }
                    finally
                    {
                        Monitor.Exit(m_syncRoot);
                    }
                }
                else
                {
                    return 0;
                }


            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            lock (m_syncRoot)
            {
                return m_list.Remove(item);
            }
        }
    }
}
