using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.Specialized
{
    public class SafeIndexedDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private Dictionary<TKey, int> m_ordinalLookup = new Dictionary<TKey,int>();
        private List<KeyValuePair<TKey, TValue>> m_list = new List<KeyValuePair<TKey, TValue>>();
        private object m_syncRoot = new object();

        public SafeIndexedDictionary()
        {
        }

        public SafeIndexedDictionary(IEqualityComparer<TKey> comparer)
            : this()
        {
        }

        public void Add(TKey key, TValue value)
        {
            lock (m_syncRoot)
            {
                var kvp = new KeyValuePair<TKey, TValue>(key, value);
                var index = m_list.Count;
                m_list.Add(kvp);
                m_ordinalLookup.Add(key, index);
            }
        }

        public TValue this[int index]
        {
            get { return m_list[index].Value; }
            set
            {
                lock (m_syncRoot)
                {
                    var existing = m_list[index];
                    m_list[index] = new KeyValuePair<TKey, TValue>(existing.Key, value);
                }
            }
        }

        public TValue this[TKey key]
        {
            get 
            {
                var index = m_ordinalLookup[key];
                return this[index];
            }
            set
            {
                var index = m_ordinalLookup[key];
                this[index] = value;
            }
        }

        public int Count
        {
            get { return m_list.Count; }
        }

        public Dictionary<TKey, TValue>.KeyCollection Keys
        {
            get 
            {
                return m_list.ToDictionary(
                    k => k.Key,
                    v => v.Value).Keys;
            }
        }

        public Dictionary<TKey, TValue>.ValueCollection Values
        {
            get
            {
                return m_list.ToDictionary(
                    k => k.Key,
                    v => v.Value).Values;
            }
        }

        public void Clear()
        {
            lock (m_syncRoot)
            {
                m_list.Clear();
                m_ordinalLookup.Clear();
            }
        }

        public bool ContainsKey(TKey key)
        {
            return m_ordinalLookup.ContainsKey(key);
        }

        public bool ContainsValue(TValue value)
        {
            foreach(var v in m_list)
            {
                if(v.Value.Equals(value)) return true;
            }
            return false;
        }

        public void Insert(int index, TKey key, TValue value)
        {
            lock (m_syncRoot)
            {
                throw new NotImplementedException();
            }
        }

        public void Remove(TKey key)
        {
            lock (m_syncRoot)
            {
                if (ContainsKey(key))
                {
                    throw new NotImplementedException();
                }
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return m_list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class OrderedDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private Dictionary<TKey, TValue> m_dictionary;
        private List<TValue> m_list = new List<TValue>();
        private object m_syncRoot = new object();

        public OrderedDictionary()
        {
            m_dictionary = new Dictionary<TKey, TValue>();
        }

        public OrderedDictionary(IEqualityComparer<TKey> comparer)
            : this()
        {
        }

        public void Add(TKey key, TValue value)
        {
            lock (m_syncRoot)
            {
                m_dictionary.Add(key, value);
                m_list.Add(value);
                var index = m_list.Count - 1;
            }
        }

        public TValue this[int index]
        {
            get { return m_list[index]; }
        }

        public TValue this[TKey key]
        {
            get { return m_dictionary[key]; }
        }

        public int Count 
        {
            get { return m_dictionary.Count; } 
        }

        public Dictionary<TKey, TValue>.KeyCollection Keys 
        {
            get { return m_dictionary.Keys; } 
        }

        public Dictionary<TKey, TValue>.ValueCollection Values 
        {
            get { return m_dictionary.Values; } 
        }

        public void Clear()
        {
            lock (m_syncRoot)
            {
                m_dictionary.Clear();
                m_list.Clear();
            }
        }

        public bool ContainsKey(TKey key)
        {
            return m_dictionary.ContainsKey(key);
        }

        public bool ContainsValue(TValue value)
        {
            return m_dictionary.ContainsValue(value);
        }

        public void Insert(int index, TKey key, TValue value)
        {
            lock (m_syncRoot)
            {
                m_list.Insert(index, value);
                m_dictionary.Add(key, value);
            }
        }

        public void Remove(TKey key)
        {
            lock (m_syncRoot)
            {
                if (ContainsKey(key))
                {
                    var existing = m_dictionary[key];
                    m_list.Remove(existing);
                    m_dictionary.Remove(key);
                }
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return m_dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
