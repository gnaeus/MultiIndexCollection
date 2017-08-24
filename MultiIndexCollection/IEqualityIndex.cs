﻿using System.Collections.Generic;

namespace MultiIndexCollection
{
    internal interface IEqualityIndex<T>
    {
        string MemberName { get; }

        IEnumerable<T> Filter(object key);

        object GetKey(T item);

        void Add(object key, T item);

        void Remove(object key, T item);

        void Clear();
    }
}
