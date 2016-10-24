using System;
using System.Collections;
using System.Collections.Generic;

namespace Utils.Helpers {
    public class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue> {
        IDictionary<TKey, TValue> data;

        public ReadOnlyDictionary(IDictionary<TKey, TValue> data) {
            this.data = data;
        }
        public void Add(TKey key, TValue value) { throw new NotSupportedException(); }
        public bool ContainsKey(TKey key) { return data.ContainsKey(key); }
        public ICollection<TKey> Keys { get { return data.Keys; } }
        public bool Remove(TKey key) { throw new NotSupportedException(); }
        public bool TryGetValue(TKey key, out TValue value) { return data.TryGetValue(key, out value); }
        public ICollection<TValue> Values { get { return data.Values; } }
        public TValue this[TKey key] {
            get { return data[key]; }
            set { throw new NotSupportedException(); }
        }
        public void Add(KeyValuePair<TKey, TValue> item) { throw new NotSupportedException(); }
        public void Clear() { throw new NotSupportedException(); }
        public bool Contains(KeyValuePair<TKey, TValue> item) { return data.Contains(item); }
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) { data.CopyTo(array, arrayIndex); }
        public int Count { get { return data.Count; } }
        public bool IsReadOnly { get { return true; } }
        public bool Remove(KeyValuePair<TKey, TValue> item) { throw new NotSupportedException(); }
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { return data.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return data.GetEnumerator(); }
    }
}
