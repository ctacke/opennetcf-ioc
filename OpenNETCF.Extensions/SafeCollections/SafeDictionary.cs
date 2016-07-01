using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;

namespace System.Collections.Generic
{
    public class SafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private Dictionary<TKey, TValue> m_dictionary;
        private readonly object m_syncRoot = new object();

        public SafeDictionary()
        {
            m_dictionary = new Dictionary<TKey, TValue>();
        }

        public SafeDictionary(IEqualityComparer<TKey> comparer)
        {
            m_dictionary = new Dictionary<TKey, TValue>(comparer);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            // instead of returning an usafe enumerator,
            // we wrap it into our thread-safe class
            lock (m_syncRoot)
            {
                return new SafeEnumerator<KeyValuePair<TKey, TValue>>(m_dictionary.GetEnumerator(), m_syncRoot);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (m_syncRoot)
            {
                m_dictionary.Add(item.Key, item.Value);
            }
        }

        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            lock (m_syncRoot)
            {
                foreach(var item in items)
                {
                    Add(item);
                }
            }
        }

        public void Clear()
        {
            lock (m_syncRoot)
            {
                m_dictionary.Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (m_syncRoot)
            {
                return m_dictionary.Contains(item);
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            lock (m_syncRoot)
            {
                var src = m_dictionary.ToArray();
                Array.Copy(src, 0, array, arrayIndex, src.Length);
            }
        }

        public int Count
        {
            get
            {
                lock (m_syncRoot)
                { 
                    return m_dictionary.Count; 
                }
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (m_syncRoot)
            {
                return m_dictionary.Remove(item.Key);
            }
        }

        public void Add(TKey key, TValue value)
        {
            lock (m_syncRoot)
            {
                m_dictionary.Add(key, value);
            }
        }

        public void AddOrReplace(TKey key, TValue value)
        {
            lock (m_syncRoot)
            {
                if (this.ContainsKey(key))
                {
                    this[key] = value;
                }
                else
                {
                    this.Add(key, value);
                }
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (m_syncRoot)
            {
                return m_dictionary.ContainsKey(key);
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                lock (m_syncRoot)
                {
                    return m_dictionary.Keys;
                }
            }
        }

        public bool Remove(TKey key)
        {
            lock (m_syncRoot)
            {
                return m_dictionary.Remove(key);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (m_syncRoot)
            {
                return m_dictionary.TryGetValue(key, out value);
            }
        }

        public ICollection<TValue> Values
        {
            get 
            {
                lock (m_syncRoot)
                {
                    return m_dictionary.Values;
                }
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                lock (m_syncRoot)
                {
                    return m_dictionary[key];
                }
            }
            set
            {
                lock (m_syncRoot)
                {
                    m_dictionary[key] = value;
                }
            }
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
