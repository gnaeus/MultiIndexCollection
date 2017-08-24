using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MultiIndexCollection
{
    public class IndexedCollection<T> : ICollection<T>
    {
        readonly List<IEqualityIndex<T>> _indexes;

        readonly Dictionary<T, List<object>> _storage;

        public int Count => _storage.Count;

        public bool IsReadOnly => false;

        public IndexedCollection()
        {
            _indexes = new List<IEqualityIndex<T>>(2);

            _storage = new Dictionary<T, List<object>>();
        }
        
        public IndexedCollection(IEnumerable<T> enumerable)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));

            _indexes = new List<IEqualityIndex<T>>(2);

            _storage = enumerable.ToDictionary(item => item, _ => new List<object>(2));
        }

        /// <exception cref="NotSupportedException" />
        public IndexedCollection<T> IndexBy<TProperty>(
            Expression<Func<T, TProperty>> property, bool isSorted = false)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            IEqualityIndex<T> index;
            if (isSorted)
            {
                index = new ComparsionIndex<T, TProperty>(property);
            }
            else
            {
                index = new EqualityIndex<T, TProperty>(property);
            }

            foreach (var pair in _storage)
            {
                T item = pair.Key;
                List<object> indexKeys = pair.Value;

                object key = index.GetKey(item);

                indexKeys.Add(key);
                index.Add(key, item);
            }

            _indexes.Add(index);

            return this;
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public IEnumerable<T> Filter(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return Filter(predicate.Body);
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        private IEnumerable<T> Filter(Expression node)
        {
            var expression = node as BinaryExpression;

            if (expression == null)
            {
                throw new NotSupportedException(
                    $"Predicate body {node} should be Binary Expression");
            }

            switch (expression.NodeType)
            {
                case ExpressionType.OrElse:
                    return Filter(expression.Left).Union(Filter(expression.Right));

                case ExpressionType.AndAlso:
                    return Filter(expression.Left).Intersect(Filter(expression.Right));

                case ExpressionType.Equal:
                    return FilterEquality(
                        expression.Left.GetMemberName(),
                        expression.Right.GetValue());

                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    return FilterComparsion(
                        expression.Left.GetMemberName(),
                        expression.Right.GetValue(),
                        expression.NodeType);

                default:
                    throw new NotSupportedException(
                        $"Predicate body {node} should be Equality or Comparsion");
            }
        }

        /// <exception cref="InvalidOperationException" />
        private IEnumerable<T> FilterEquality(string memberName, object key)
        {
            IEqualityIndex<T> index = _indexes.FirstOrDefault(i => i.MemberName == memberName);

            if (index == null)
            {
                throw new InvalidOperationException($"There is no index for property '{memberName}'");
            }

            return index.Filter(key);
        }

        /// <exception cref="InvalidOperationException" />
        private IEnumerable<T> FilterComparsion(
            string memberName, object key, ExpressionType type)
        {
            IComparsionIndex<T> index = _indexes
                .OfType<IComparsionIndex<T>>()
                .FirstOrDefault(i => i.MemberName == memberName);

            if (index == null)
            {
                throw new InvalidOperationException($"There is no comparsion index for property '{memberName}'");
            }

            return index.Filter(key, type);
        }
        
        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            if (_storage.TryGetValue(item, out List<object> indexKeys))
            {
                for (int i = 0; i < _indexes.Count; i++)
                {
                    IEqualityIndex<T> index = _indexes[i];
                    object currentKey = index.GetKey(item);
                    object lastKey = indexKeys[i];

                    if (lastKey != currentKey)
                    {
                        indexKeys[i] = currentKey;
                        index.Remove(lastKey, item);
                        index.Add(currentKey, item);
                    }
                }
            }
            else
            {
                indexKeys = new List<object>(_indexes.Count);

                foreach (IEqualityIndex<T> index in _indexes)
                {
                    object key = index.GetKey(item);

                    indexKeys.Add(key);
                    index.Add(key, item);
                }

                _storage.Add(item, indexKeys);
            }
        }

        public bool Remove(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            if (_storage.TryGetValue(item, out List<object> indexKeys))
            {
                for (int i = 0; i < _indexes.Count; i++)
                {
                    IEqualityIndex<T> index = _indexes[i];
                    object lastKey = indexKeys[i];

                    index.Remove(lastKey, item);
                }

                _storage.Remove(item);

                return true;
            }
            else
            {
                return false;
            }
        }

        public void Update(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            Add(item);
        }

        public void Clear()
        {
            foreach (IEqualityIndex<T> index in _indexes)
            {
                _indexes.Clear();
            }
            _storage.Clear();
        }

        public bool Contains(T item)
        {
            return _storage.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _storage.Keys.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _storage.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
