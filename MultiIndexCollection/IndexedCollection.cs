using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace MultiIndexCollection
{
    /// <summary>
    /// Generic collection with support for indexing by multiple properties.
    /// </summary>
    /// <typeparam name="T"> The type of elements in the collection. </typeparam>
    public class IndexedCollection<T> : ICollection<T>, IReadOnlyCollection<T>, ICollection
    {
        readonly List<IEqualityIndex<T>> _indexes;

        readonly Dictionary<T, List<object>> _storage;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="IndexedCollection{T}"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="IndexedCollection{T}"/>.
        /// </returns>
        public int Count => _storage.Count;
        
        bool ICollection<T>.IsReadOnly => false;
        
        bool ICollection.IsSynchronized => false;
        
        object ICollection.SyncRoot => ((ICollection)_storage).SyncRoot;

        /// <summary>
        /// Initializes a new instance of <see cref="IndexedCollection{T}"/> that is empty.
        /// </summary>
        public IndexedCollection()
        {
            _indexes = new List<IEqualityIndex<T>>(2);

            _storage = new Dictionary<T, List<object>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexedCollection{T}"/> that
        /// contains elements copied from the specified <paramref name="enumerable"/>.
        /// </summary>
        /// <param name="enumerable"> The enumerable whose elements are copied to the new collection. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="enumerable"/> is null. </exception>
        public IndexedCollection(IEnumerable<T> enumerable)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));

            _indexes = new List<IEqualityIndex<T>>(2);

            _storage = enumerable.ToDictionary(item => item, _ => new List<object>(2));

            foreach (var pair in _storage)
            {
                if (pair.Key is INotifyPropertyChanged observable)
                {
                    observable.PropertyChanged += OnPropertyChanged;
                }
            }

            if (enumerable is INotifyCollectionChanged observableCollection)
            {
                observableCollection.CollectionChanged += OnCollectionChanged;
            }
        }

        #region Indexing

        /// <summary>
        /// Add an index to the <see cref="IndexedCollection{T}"/>.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property <paramref name="property"/>. </typeparam>
        /// <param name="property"> An expression to locate property in collection element. </param>
        /// <param name="isSorted"> Should index be sorted or not. </param>
        /// <returns> This instance of <see cref="IndexedCollection{T}"/>. </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="property"/> is null. </exception>
        /// <exception cref="NotSupportedException"> Expression <paramref name="property"/> is not a Member Access. </exception>
        public IndexedCollection<T> IndexBy<TProperty>(
            Expression<Func<T, TProperty>> property, bool isSorted = false)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            if (isSorted)
            {
                IndexBy(new ComparsionIndex<T, TProperty>(property));
            }
            else
            {
                IndexBy(new EqualityIndex<T, TProperty>(property));
            }
            
            return this;
        }

        /// <summary>
        /// Add an index that performs a case-insensitive ordinal string comparison to the <see cref="IndexedCollection{T}"/>.
        /// </summary>
        /// <param name="property"> An expression to locate string property in collection element. </param>
        /// <returns> This instance of <see cref="IndexedCollection{T}"/>. </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="property"/> is null. </exception>
        /// <exception cref="NotSupportedException"> Expression <paramref name="property"/> is not a Member Access. </exception>
        public IndexedCollection<T> IndexByIgnoreCase(Expression<Func<T, string>> property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            
            IndexBy(new ComparsionIndex<T, string>(property, StringComparer.OrdinalIgnoreCase));

            return this;
        }

        private void IndexBy(IEqualityIndex<T> index)
        {
            foreach (var pair in _storage)
            {
                T item = pair.Key;
                List<object> indexKeys = pair.Value;

                object key = index.GetKey(item);

                indexKeys.Add(key);
                index.Add(key, item);
            }

            _indexes.Add(index);
        }

        #endregion

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        private IComparsionIndex<T> FindComparsionIndex(Expression memberExpression)
        {
            string memberName = memberExpression.GetMemberName();

            var index = (IComparsionIndex<T>)_indexes
                .Find(i => i.MemberName == memberName && i is IComparsionIndex<T>);

            if (index == null)
            {
                throw new InvalidOperationException(
                    $"There is no comparsion index for property '{memberName}'");
            }

            return index;
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        private ILookup<TProperty, T> FindLookup<TProperty>(Expression memberExpression)
        {
            string memberName = memberExpression.GetMemberName();
            
            var lookup = (ILookup<TProperty, T>)_indexes
                .Find(i => i.MemberName == memberName && i is ILookup<TProperty, T>);

            if (lookup == null)
            {
                throw new InvalidOperationException($"There is no index for property '{memberName}'");
            }

            return lookup;
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        private ILookup<TProperty, T> FindEqualityLookup<TProperty>(Expression memberExpression)
        {
            string memberName = memberExpression.GetMemberName();

            var lookup = (ILookup<TProperty, T>)_indexes
                .Find(i => i.MemberName == memberName && i is EqualityIndex<T, TProperty>);

            if (lookup == null)
            {
                throw new InvalidOperationException($"There is no equality index for property '{memberName}'");
            }

            return lookup;
        }

        #region LINQ Methods

        public IEnumerable<T> AsEnumerable()
        {
            return this;
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public T First(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return Filter(predicate.Body).First();
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public T FirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return Filter(predicate.Body).FirstOrDefault();
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public ILookup<TProperty, T> GroupBy<TProperty>(Expression<Func<T, TProperty>> property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            return FindLookup<TProperty>(property.Body);
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public IEnumerable<TResult> GroupJoin<TOuter, TKey, TResult>(
            IEnumerable<TOuter> outerEnumerable,
            Expression<Func<T, TKey>> innerKeySelector,
            Func<TOuter, TKey> outerKeySelector,
            Func<IEnumerable<T>, TOuter, TResult> resultSelector)
        {
            if (outerEnumerable == null) throw new ArgumentNullException(nameof(outerEnumerable));
            if (innerKeySelector == null) throw new ArgumentNullException(nameof(innerKeySelector));
            if (outerKeySelector == null) throw new ArgumentNullException(nameof(outerKeySelector));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

            ILookup<TKey, T> lookup = FindEqualityLookup<TKey>(innerKeySelector.Body);

            foreach (TOuter outer in outerEnumerable)
            {
                IEnumerable<T> innerEnumerable = lookup[outerKeySelector.Invoke(outer)];

                yield return resultSelector.Invoke(innerEnumerable, outer);
            }
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public IEnumerable<TResult> Join<TOuter, TKey, TResult>(
            IEnumerable<TOuter> outerEnumerable,
            Expression<Func<T, TKey>> innerKeySelector,
            Func<TOuter, TKey> outerKeySelector,
            Func<T, TOuter, TResult> resultSelector)
        {
            if (outerEnumerable == null) throw new ArgumentNullException(nameof(outerEnumerable));
            if (innerKeySelector == null) throw new ArgumentNullException(nameof(innerKeySelector));
            if (outerKeySelector == null) throw new ArgumentNullException(nameof(outerKeySelector));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

            ILookup<TKey, T> lookup = FindEqualityLookup<TKey>(innerKeySelector.Body);

            foreach (TOuter outer in outerEnumerable)
            {
                foreach (T inner in lookup[outerKeySelector.Invoke(outer)])
                {
                    yield return resultSelector.Invoke(inner, outer);
                }
            }
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public IEnumerable<T> HavingMax<TProperty>(Expression<Func<T, TProperty>> property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            IComparsionIndex<T> index = FindComparsionIndex(property.Body);

            return index.HavingMax();
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public IEnumerable<T> HavingMin<TProperty>(Expression<Func<T, TProperty>> property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            IComparsionIndex<T> index = FindComparsionIndex(property.Body);

            return index.HavingMin();
        }
        
        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public T Last(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return Filter(predicate.Body).Last();
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public T LastOrDefault(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return Filter(predicate.Body).LastOrDefault();
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public long LongCount(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return Filter(predicate.Body).LongCount();
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public TProperty Max<TProperty>(Expression<Func<T, TProperty>> property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            IComparsionIndex<T> index = FindComparsionIndex(property.Body);

            return (TProperty)index.Max();
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public TProperty Min<TProperty>(Expression<Func<T, TProperty>> property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            IComparsionIndex<T> index = FindComparsionIndex(property.Body);

            return (TProperty)index.Min();
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public IEnumerable<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            return FindComparsionIndex(property.Body);   
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public IEnumerable<T> OrderByDescending<TProperty>(Expression<Func<T, TProperty>> property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            return FindComparsionIndex(property.Body).Reverse();
        }
        
        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public T Single(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return Filter(predicate.Body).Single();
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public T SingleOrDefault(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return Filter(predicate.Body).SingleOrDefault();
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public ILookup<TProperty, T> ToLookup<TProperty>(Expression<Func<T, TProperty>> property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            return FindLookup<TProperty>(property.Body);
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public IEnumerable<T> Where(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return Filter(predicate.Body);
        }

        #endregion

        #region Filtering

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        private IEnumerable<T> Filter(Expression body)
        {
            if (body is MethodCallExpression methodCall)
            {
                return FilterStringStartsWith(methodCall);
            }

            var binary = body as BinaryExpression;

            if (binary == null)
            {
                throw new NotSupportedException(
                    $"Predicate body {body} should be Binary Expression");
            }

            switch (binary.NodeType)
            {
                case ExpressionType.OrElse:
                    return Filter(binary.Left).Union(Filter(binary.Right));

                case ExpressionType.AndAlso:
                    return FilterRangeOrAndAlso(binary.Left, binary.Right);

                case ExpressionType.Equal:
                    return FilterEquality(binary.Left, binary.Right);

                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    return FilterComparsion(binary.Left, binary.Right, binary.NodeType);

                default:
                    throw new NotSupportedException(
                        $"Predicate body {body} should be Equality or Comparsion");
            }
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        private IEnumerable<T> FilterEquality(Expression memberExpression, Expression keyExpression)
        {
            string memberName = memberExpression.GetMemberName();

            IEqualityIndex<T> index = _indexes.Find(i => i.MemberName == memberName);

            if (index == null)
            {
                throw new InvalidOperationException($"There is no index for property '{memberName}'");
            }

            return index.Filter(keyExpression.GetValue());
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        private IEnumerable<T> FilterComparsion(
            Expression memberExpression, Expression keyExpression, ExpressionType type)
        {
            IComparsionIndex<T> index = FindComparsionIndex(memberExpression);

            object key = keyExpression.GetValue();

            switch (type)
            {
                case ExpressionType.GreaterThan:
                    return index.GreaterThan(key, exclusive: true);

                case ExpressionType.GreaterThanOrEqual:
                    return index.GreaterThan(key, exclusive: false);

                case ExpressionType.LessThan:
                    return index.LessThan(key, exclusive: true);

                case ExpressionType.LessThanOrEqual:
                    return index.LessThan(key, exclusive: false);

                default:
                    throw new NotSupportedException($"Expression {type} should be Comparsion");
            }
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        private IEnumerable<T> FilterRangeOrAndAlso(Expression left, Expression right)
        {
            var leftOperation = left as BinaryExpression;

            if (leftOperation == null)
            {
                throw new NotSupportedException(
                    $"Predicate body {left} should be Binary Expression");
            }

            var rightOperation = right as BinaryExpression;

            if (rightOperation == null)
            {
                throw new NotSupportedException(
                    $"Predicate body {right} should be Binary Expression");
            }

            string leftMemberName = leftOperation.Left.GetMemberName();
            string rightMemberName = rightOperation.Left.GetMemberName();

            if (leftMemberName == rightMemberName)
            {
                var index = (IComparsionIndex<T>)_indexes
                    .Find(i => i.MemberName == leftMemberName && i is IComparsionIndex<T>);

                if (index != null)
                {
                    switch (leftOperation.NodeType)
                    {
                        case ExpressionType.GreaterThan:
                            switch (rightOperation.NodeType)
                            {
                                case ExpressionType.LessThan:
                                    return index.Between(
                                        leftOperation.Right.GetValue(), true,
                                        rightOperation.Right.GetValue(), true);
                                
                                case ExpressionType.LessThanOrEqual:
                                    return index.Between(
                                        leftOperation.Right.GetValue(), true,
                                        rightOperation.Right.GetValue(), false);
                            }
                            break;

                        case ExpressionType.GreaterThanOrEqual:
                            switch (rightOperation.NodeType)
                            {
                                case ExpressionType.LessThan:
                                    return index.Between(
                                        leftOperation.Right.GetValue(), false,
                                        rightOperation.Right.GetValue(), true);

                                case ExpressionType.LessThanOrEqual:
                                    return index.Between(
                                        leftOperation.Right.GetValue(), false,
                                        rightOperation.Right.GetValue(), false);
                            }
                            break;

                        case ExpressionType.LessThan:
                            switch (rightOperation.NodeType)
                            {
                                case ExpressionType.GreaterThan:
                                    return index.Between(
                                        rightOperation.Right.GetValue(), true,
                                        leftOperation.Right.GetValue(), true);

                                case ExpressionType.GreaterThanOrEqual:
                                    return index.Between(
                                        rightOperation.Right.GetValue(), false,
                                        leftOperation.Right.GetValue(), true);
                            }
                            break;

                        case ExpressionType.LessThanOrEqual:
                            switch (rightOperation.NodeType)
                            {
                                case ExpressionType.GreaterThan:
                                    return index.Between(
                                        rightOperation.Right.GetValue(), true,
                                        leftOperation.Right.GetValue(), false);

                                case ExpressionType.GreaterThanOrEqual:
                                    return index.Between(
                                        rightOperation.Right.GetValue(), false,
                                        leftOperation.Right.GetValue(), false);
                            }
                            break;
                    }
                }
            }
            
            return Filter(left).Intersect(Filter(right));
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        private IEnumerable<T> FilterStringStartsWith(MethodCallExpression methodCall)
        {
            if (methodCall.Method.DeclaringType == typeof(String) &&
                methodCall.Method.Name == nameof(String.StartsWith))
            {
                IComparsionIndex<T> index = FindComparsionIndex(methodCall.Object);

                string keyFrom = (string)methodCall.Arguments.First().GetValue();

                if (String.IsNullOrEmpty(keyFrom))
                {
                    return index.GreaterThan(keyFrom, false);
                }

                char lastChar = keyFrom[keyFrom.Length - 1];

                string keyTo = keyFrom.Substring(0, keyFrom.Length - 1) + (char)(lastChar + 1);

                return index.Between(keyFrom, false, keyTo, true);
            }

            throw new NotSupportedException(
                $"Predicate body {methodCall} should be String.StartsWith()");
        }

        #endregion

        #region Updating

        /// <summary>
        /// Adds an element to the <see cref="IndexedCollection{T}"/>.
        /// </summary>
        /// <param name="item"> The element to add to the <see cref="IndexedCollection{T}"/>. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="item"/> is null. </exception>
        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            AddOrUpdate(item);
        }

        /// <summary>
        /// Apply changes in indexed properties of the specified <paramref name="item"/>
        /// to the <see cref="IndexedCollection{T}"/> indexes.
        /// </summary>
        /// <param name="item"> The element to update. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="item"/> is null. </exception>
        public void Update(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            AddOrUpdate(item);
        }

        private void AddOrUpdate(T item)
        {
            if (_storage.TryGetValue(item, out List<object> indexKeys))
            {
                for (int i = 0; i < _indexes.Count; i++)
                {
                    IEqualityIndex<T> index = _indexes[i];
                    object currentKey = index.GetKey(item);
                    object lastKey = indexKeys[i];

                    if (!Equals(lastKey, currentKey))
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

                if (item is INotifyPropertyChanged observable)
                {
                    observable.PropertyChanged += OnPropertyChanged;
                }
            }
        }

        /// <summary>
        /// Removes the specified element from a <see cref="IndexedCollection{T}"/>.
        /// </summary>
        /// <param name="item"> The element to remove. </param>
        /// <returns>
        /// true if the element is successfully found and removed; otherwise, false.
        /// This method returns false if item is not found in the <see cref="IndexedCollection{T}"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="item"/> is null. </exception>
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

                if (item is INotifyPropertyChanged observable)
                {
                    observable.PropertyChanged -= OnPropertyChanged;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Removes all items from the <see cref="IndexedCollection{T}"/>.
        /// </summary>
        public void Clear()
        {
            foreach (IEqualityIndex<T> index in _indexes)
            {
                index.Clear();
            }

            foreach (var pair in _storage)
            {
                if (pair.Key is INotifyPropertyChanged observable)
                {
                    observable.PropertyChanged -= OnPropertyChanged;
                }
            }

            _storage.Clear();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            int pointer = _indexes.FindIndex(i => i.MemberName == e.PropertyName);

            if (pointer != -1 && sender is T item &&
                _storage.TryGetValue(item, out List<object> indexKeys))
            {
                IEqualityIndex<T> index = _indexes[pointer];
                object currentKey = index.GetKey(item);
                object lastKey = indexKeys[pointer];

                indexKeys[pointer] = currentKey;
                index.Remove(lastKey, item);
                index.Add(currentKey, item);
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddItems(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveItems(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    RemoveItems(e.OldItems);
                    AddItems(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ResetItems(sender);
                    break;
            }
        }

        private void AddItems(IEnumerable newItems)
        {
            foreach (object newItem in newItems)
            {
                if (newItem is T item)
                {
                    AddOrUpdate(item);
                }
            }
        }

        private void RemoveItems(IEnumerable oldItems)
        {
            foreach (object oldItem in oldItems)
            {
                if (oldItem is T item)
                {
                    Remove(item);
                }
            }
        }

        private void ResetItems(object sender)
        {
            Clear();

            if (sender is IEnumerable<T> items)
            {
                foreach (T item in items)
                {
                    AddOrUpdate(item);
                }
            }
        }

        #endregion

        /// <summary>
        /// Determines whether the <see cref="IndexedCollection{T}"/> contains a specific value.
        /// </summary>
        /// <param name="item"> The object to locate in the <see cref="IndexedCollection{T}"/>. </param>
        /// <returns> true if item is found in the <see cref="IndexedCollection{T}"/>; otherwise, false. </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="item"/> is null. </exception>
        public bool Contains(T item)
        {
            return _storage.ContainsKey(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="IndexedCollection{T}"/> to an <see cref="Array"/>
        /// starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from
        /// <see cref="IndexedCollection{T}"/>. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException"> <paramref name="array"/> is null. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="arrayIndex"/> is less than 0. </exception>
        /// <exception cref="ArgumentException">
        /// The number of elements in the source <see cref="IndexedCollection{T}"/> is greater than
        /// the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
        /// </exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _storage.Keys.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _storage.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)_storage.Keys).CopyTo(array, index);
        }
    }
}
