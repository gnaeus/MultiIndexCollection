﻿using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace MultiIndexCollection.Benchmarks
{
    // [MemoryDiagnoser]
    public class LinqMethodsBenchmark
    {
        public class Entity
        {
            public int Property { get; set; }
        }

        [Params(100, 300, 1000, 3000, 10000, 30000, 100000, 300000, 1000000)]
        public int Length { get; set; } = 10000;
        
        private IList<Entity> _outer;
        private IList<Entity> _list;

        private IndexedCollection<Entity> _hashIndexed;
        private IndexedCollection<Entity> _sortedIndexed;

        private ILookup<int, Entity> _lookup;
        private ILookup<int, Entity> _hashLookup;
        private ILookup<int, Entity> _sortedLookup;

        private int _maxProperty = 0;
        private const int _maxPropertyDuplicates = 3;

        [GlobalSetup]
        public void GlobalSetup()
        {

            var random = new Random();

            _outer = new List<Entity>(Length);
            
            for (int i = 0; i < Length && _outer.Count < Length; i++)
            {
                int duplicatesCount = random.Next(_maxPropertyDuplicates) + 1;

                for (int j = 0; j < duplicatesCount && _outer.Count < Length; j++)
                {
                    _outer.Add(new Entity { Property = i });
                }
            }

            _outer.Shuffle();

            _list = new List<Entity>(Length);
            
            for (int i = 0; i < Length && _list.Count < Length; i++)
            {
                int duplicatesCount = random.Next(_maxPropertyDuplicates) + 1;

                for (int j = 0; j < duplicatesCount && _list.Count < Length; j++)
                {
                    _list.Add(new Entity { Property = i });
                }
                _maxProperty = i;
            }

            _list.Shuffle();

            _hashIndexed = _list.IndexBy(e => e.Property);

            _sortedIndexed = _list.IndexBy(e => e.Property, true);

            _lookup = _list.ToLookup(e => e.Property);

            _hashLookup = _hashIndexed.ToLookup(e => e.Property);

            _sortedLookup = _sortedIndexed.ToLookup(e => e.Property);
        }

        [Benchmark]
        public object LinqGroupBy()
        {
            return _list.GroupBy(e => e.Property).Enumerate();
        }

        [Benchmark]
        public object HashIndexGroupBy()
        {
            return _hashIndexed.GroupBy(e => e.Property).Enumerate();
        }

        [Benchmark]
        public object SortedIndexGroupBy()
        {
            return _sortedIndexed.GroupBy(e => e.Property).Enumerate();
        }

        [Benchmark]
        public int LinqGroupJoin()
        {
            return _outer
                .GroupJoin(_list,
                    o => o.Property, i => i.Property,
                    (o, i) => o.Property + i.Sum(e => e.Property))
                .Enumerate();
        }

        [Benchmark]
        public int IndexedGroupJoin()
        {
            return _hashIndexed
                .GroupJoin(_outer,
                    i => i.Property, o => o.Property,
                    (i, o) => i.Sum(e => e.Property) + o.Property)
                .Enumerate();
        }

        [Benchmark]
        public Entity LinqHavingMax()
        {
            int max = _list.Max(e => e.Property);

            return _list.Where(e => e.Property == max).Enumerate();
        }

        [Benchmark]
        public Entity IndexedHavingMax()
        {
            return _sortedIndexed.HavingMax(e => e.Property).Enumerate();
        }

        [Benchmark]
        public Entity LinqHavingMin()
        {
            int min = _list.Min(e => e.Property);

            return _list.Where(e => e.Property == min).Enumerate();
        }

        [Benchmark]
        public Entity IndexedHavingMin()
        {
            return _sortedIndexed.HavingMin(e => e.Property).Enumerate();
        }

        [Benchmark]
        public int LinqJoin()
        {
            return _outer
                .Join(_list,
                    o => o.Property, i => i.Property,
                    (o, i) => o.Property + i.Property)
                .Enumerate();
        }

        [Benchmark]
        public int IndexedJoin()
        {
            return _hashIndexed
                .Join(_outer,
                    i => i.Property, o => o.Property,
                    (i, o) => i.Property + o.Property)
                .Enumerate();
        }

        [Benchmark]
        public int LinqMax()
        {
            return _list.Min(e => e.Property);
        }

        [Benchmark]
        public int IndexedMax()
        {
            return _sortedIndexed.Max(e => e.Property);
        }

        [Benchmark]
        public int LinqMin()
        {
            return _list.Min(e => e.Property);
        }

        [Benchmark]
        public int IndexedMin()
        {
            return _sortedIndexed.Min(e => e.Property);
        }

        [Benchmark]
        public Entity LinqOrderBy()
        {
            return _list.OrderBy(e => e.Property).Enumerate();
        }

        [Benchmark]
        public Entity IndexedOrderBy()
        {
            return _sortedIndexed.OrderBy(e => e.Property).Enumerate();
        }

        [Benchmark]
        public Entity LinqOrderByDescending()
        {
            return _list.OrderByDescending(e => e.Property).Enumerate();
        }

        [Benchmark]
        public Entity IndexedOrderByDescending()
        {
            return _sortedIndexed.OrderByDescending(e => e.Property).Enumerate();
        }

        private int _i = 0;

        [Benchmark]
        public bool LinqLookup()
        {
            _i = (_i + 1) % _maxProperty;
            return _lookup.Contains(_i);
        }

        [Benchmark]
        public bool HashIndexLookup()
        {
            _i = (_i + 1) % _maxProperty;
            return _hashLookup.Contains(_i);
        }

        [Benchmark]
        public bool SortedIndexLookup()
        {
            _i = (_i + 1) % _maxProperty;
            return _sortedLookup.Contains(_i);
        }
    }
}
