using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MultiIndexCollection
{
    internal class EqualityIndex<T, TProperty> : PropertyIndex<T, TProperty>, IEqualityIndex<T>
    {
        // TODO: store items with `null` key

        readonly Dictionary<TProperty, object> _storage;

        /// <exception cref="NotSupportedException" />
        public EqualityIndex(Expression<Func<T, TProperty>> lambda)
            : base(lambda)
        {
            _storage = new Dictionary<TProperty, object>();
        }

        public IEnumerable<T> Filter(object key)
        {
            if (_storage.TryGetValue((TProperty)key, out object bucket))
            {
                return bucket is T
                    ? new[] { (T)bucket }
                    : (IEnumerable<T>)bucket;
            }

            return Enumerable.Empty<T>();
        }

        public void Add(object key, T item)
        {
            var propKey = (TProperty)key;

            if (_storage.TryGetValue(propKey, out object bucket))
            {
                if (bucket is T)
                {
                    _storage[propKey] = new List<T>(2) { (T)bucket, item };
                }
                else if (bucket is List<T> list)
                {
                    if (list.Count < 16)
                    {
                        list.Add(item);
                    }
                    else
                    {
                        _storage[propKey] = new HashSet<T>(list) { item };
                    }
                }
                else
                {
                    var hashSet = (HashSet<T>)bucket;

                    hashSet.Add(item);
                }
            }
            else
            {
                _storage.Add(propKey, item);
            }
        }

        public void Remove(object key, T item)
        {
            var propKey = (TProperty)key;

            object bucket = _storage[propKey];

            if (bucket is T)
            {
                _storage.Remove(propKey);
            }
            else if (bucket is List<T> list)
            {
                list.Remove(item);

                if (list.Count == 1)
                {
                    _storage[propKey] = list[0];
                }
            }
            else
            {
                var hashSet = (HashSet<T>)bucket;

                hashSet.Remove(item);

                if (hashSet.Count == 16)
                {
                    _storage[propKey] = new List<T>(hashSet);
                }
            }
        }

        public void Clear()
        {
            _storage.Clear();
        }
    }
}
