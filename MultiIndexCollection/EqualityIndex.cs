using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MultiIndexCollection
{
    internal class EqualityIndex<T, TProperty> : PropertyIndex<T, TProperty>, IEqualityIndex<T>
    {
        private object _nullBucket;

        readonly Dictionary<TProperty, object> _buckets;

        /// <exception cref="NotSupportedException" />
        public EqualityIndex(Expression<Func<T, TProperty>> lambda)
            : base(lambda)
        {
            _buckets = new Dictionary<TProperty, object>();
        }

        public IEnumerable<T> Filter(object key)
        {
            if (key == null)
            {
                if (_nullBucket != null)
                {
                    return _nullBucket is T element
                        ? new[] { element }
                        : (IEnumerable<T>)_nullBucket;
                }
            }
            else if (_buckets.TryGetValue((TProperty)key, out object bucket))
            {
                return bucket is T element
                    ? new[] { element }
                    : (IEnumerable<T>)bucket;
            }

            return Enumerable.Empty<T>();
        }

        public void Add(object key, T item)
        {
            if (key == null)
            {
                if (_nullBucket is HashSet<T> hashSet)
                {
                    hashSet.Add(item);
                }
                else if (_nullBucket is List<T> list)
                {
                    if (list.Count < 16)
                    {
                        list.Add(item);
                    }
                    else
                    {
                        _nullBucket = new HashSet<T>(list) { item };
                    }   
                }
                else if (_nullBucket is T element)
                {
                    _nullBucket = new List<T>(2) { element, item };
                }
                else
                {
                    _nullBucket = item;
                }
                
                return;
            }

            var propKey = (TProperty)key;

            if (_buckets.TryGetValue(propKey, out object bucket))
            {
                if (bucket is T element)
                {
                    _buckets[propKey] = new List<T>(2) { element, item };
                }
                else if (bucket is List<T> list)
                {
                    if (list.Count < 16)
                    {
                        list.Add(item);
                    }
                    else
                    {
                        _buckets[propKey] = new HashSet<T>(list) { item };
                    }
                }
                else if (bucket is HashSet<T> hashSet)
                {
                    hashSet.Add(item);
                }
            }
            else
            {
                _buckets.Add(propKey, item);
            }
        }

        public void Remove(object key, T item)
        {
            if (key == null)
            {
                if (_nullBucket is HashSet<T> hashSet)
                {
                    hashSet.Remove(item);

                    if (hashSet.Count == 16)
                    {
                        _nullBucket = new List<T>(hashSet);
                    }
                }
                else if (_nullBucket is List<T> list)
                {
                    list.Remove(item);

                    if (list.Count == 1)
                    {
                        _nullBucket = list[0];
                    }
                }
                else if (_nullBucket is T)
                {
                    _nullBucket = null;
                }

                return;
            }

            var propKey = (TProperty)key;

            object bucket = _buckets[propKey];

            if (bucket is T)
            {
                _buckets.Remove(propKey);
            }
            else if (bucket is List<T> list)
            {
                list.Remove(item);

                if (list.Count == 1)
                {
                    _buckets[propKey] = list[0];
                }
            }
            else if (bucket is HashSet<T> hashSet)
            {
                hashSet.Remove(item);

                if (hashSet.Count == 16)
                {
                    _buckets[propKey] = new List<T>(hashSet);
                }
            }
        }

        public void Clear()
        {
            _nullBucket = null;
            _buckets.Clear();
        }
    }
}
