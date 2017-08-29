﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FastExpressionCompiler;
using System.Linq;

namespace MultiIndexCollection
{
    internal class ComparsionIndex<T, TProperty> : SortedSet<KeyValuePair<TProperty, object>>, IComparsionIndex<T>
    {
        public string MemberName { get; }

        readonly Func<T, TProperty> _getKey;

        private object _nullBucket;

        const int MaxListBucketCount = 16;

        /// <exception cref="NotSupportedException" />
        public ComparsionIndex(Expression<Func<T, TProperty>> lambda)
            : base(KeyComparer)
        {
            var memberExpression = lambda.Body as MemberExpression;

            if (memberExpression == null || memberExpression.NodeType != ExpressionType.MemberAccess)
            {
                throw new NotSupportedException($"Expression {lambda} is not a Member Access");
            }

            MemberName = memberExpression.Member.Name;

            _getKey = lambda.CompileFast();
        }

        public object GetKey(T item)
        {
            return _getKey.Invoke(item);
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
            else
            {
                var pairKey = new KeyValuePair<TProperty, object>((TProperty)key, null);

                object bucket = GetViewBetween(pairKey, pairKey).FirstOrDefault().Value;

                if (bucket != null)
                {
                    return bucket is T element
                        ? new[] { element }
                        : (IEnumerable<T>)bucket;
                }
            }

            return Enumerable.Empty<T>();
        }

        public IEnumerable<T> Filter(object key, ExpressionType type)
        {
            throw new NotImplementedException();
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
                    if (list.Count < MaxListBucketCount)
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

            var pairKey = new KeyValuePair<TProperty, object>(propKey, null);
            
            if (Contains(pairKey))
            {
                object bucket = GetViewBetween(pairKey, pairKey).FirstOrDefault().Value;

                if (bucket is T element)
                {
                    Remove(pairKey);
                    Add(new KeyValuePair<TProperty, object>(propKey, new List<T>(2) { element, item }));
                }
                else if (bucket is List<T> list)
                {
                    if (list.Count < MaxListBucketCount)
                    {
                        list.Add(item);
                    }
                    else
                    {
                        Remove(pairKey);
                        Add(new KeyValuePair<TProperty, object>(propKey, new HashSet<T>(list) { item }));
                    }
                }
                else if (bucket is HashSet<T> hashSet)
                {
                    hashSet.Add(item);
                }
            }
            else
            {
                Add(new KeyValuePair<TProperty, object>(propKey, item));
            }
        }

        public void Remove(object key, T item)
        {
            if (key == null)
            {
                if (_nullBucket is HashSet<T> hashSet)
                {
                    hashSet.Remove(item);

                    if (hashSet.Count == MaxListBucketCount)
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

            var pairKey = new KeyValuePair<TProperty, object>(propKey, null);

            if (Contains(pairKey))
            {
                object bucket = GetViewBetween(pairKey, pairKey).FirstOrDefault().Value;

                if (bucket is T)
                {
                    Remove(pairKey);
                }
                else if (bucket is List<T> list)
                {
                    list.Remove(item);

                    if (list.Count == 1)
                    {
                        Remove(pairKey);
                        Add(new KeyValuePair<TProperty, object>(propKey, list[0]));
                    }
                }
                else if (bucket is HashSet<T> hashSet)
                {
                    hashSet.Remove(item);

                    if (hashSet.Count == MaxListBucketCount)
                    {
                        Remove(pairKey);
                        Add(new KeyValuePair<TProperty, object>(propKey, new List<T>(hashSet)));
                    }
                }
            }
        }

        public new void Clear()
        {
            base.Clear();

            _nullBucket = null;
        }

        static readonly KeyValueComparer KeyComparer = new KeyValueComparer();

        private class KeyValueComparer : IComparer<KeyValuePair<TProperty, object>>
        {
            readonly Comparer<TProperty> _keyComparer = Comparer<TProperty>.Default;

            public int Compare(KeyValuePair<TProperty, object> x, KeyValuePair<TProperty, object> y)
            {
                return _keyComparer.Compare(x.Key, y.Key);
            }
        }
    }
}
