using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace MultiIndexCollection.Benchmarks
{
    [MemoryDiagnoser]
    public class LinqMethodsBenchmark
    {
        class Entity
        {
            public int Property { get; set; }
        }

        [Params(100, 1000, 10000, 100000)]
        public int Length { get; set; }

        [Params(3)]
        public int MaxDuplicates { get; set; }

        private IList<Entity> _outer;
        private IList<Entity> _inner;

        private IndexedCollection<Entity> _hashIndex;
        private IndexedCollection<Entity> _sortedIndex;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var random = new Random();

            _outer = new List<Entity>(Length);
            
            for (int i = 0; i < Length && _outer.Count < Length; i++)
            {
                for (int j = 0; j < random.Next(MaxDuplicates + 1); j++)
                {
                    _outer.Add(new Entity { Property = i });
                }
            }

            _outer.Shuffle();

            _inner = new List<Entity>(Length);
            
            for (int i = 0; i < Length && _inner.Count < Length; i++)
            {
                for (int j = 0; j < random.Next(MaxDuplicates + 1); j++)
                {
                    _inner.Add(new Entity { Property = i });
                }
            }

            _inner.Shuffle();

            _hashIndex = _inner.IndexBy(e => e.Property);

            _sortedIndex = _inner.IndexBy(e => e.Property, true);
        }

        [Benchmark]
        public int LinqJoin()
        {
            return _outer
                .Join(_inner,
                    o => o.Property, i => i.Property,
                    (o, i) => o.Property + i.Property)
                .Sum();
        }

        [Benchmark]
        public int IndexedJoin()
        {
            return _hashIndex
                .Join(_outer,
                    i => i.Property, o => o.Property,
                    (i, o) => i.Property + o.Property)
                .Sum();
        }

        [Benchmark]
        public int LinqGroupJoin()
        {
            return _outer
                .GroupJoin(_inner,
                    o => o.Property, i => i.Property,
                    (o, i) => o.Property + i.Sum(e => e.Property))
                .Sum();
        }

        [Benchmark]
        public int IndexedGroupJoin()
        {
            return _hashIndex
                .GroupJoin(_outer,
                    i => i.Property, o => o.Property,
                    (i, o) => i.Sum(e => e.Property) + o.Property)
                .Sum();
        }

        [Benchmark]
        public int LinqOrderBy()
        {
            return _inner.OrderBy(e => e.Property).Sum(e => e.Property);
        }

        [Benchmark]
        public int IndexedOrderBy()
        {
            return _sortedIndex.OrderBy(e => e.Property).Sum(e => e.Property);
        }

        [Benchmark]
        public int LinqOrderByDescending()
        {
            return _inner.OrderByDescending(e => e.Property).Sum(e => e.Property);
        }

        [Benchmark]
        public int IndexedOrderByDescending()
        {
            return _sortedIndex.OrderByDescending(e => e.Property).Sum(e => e.Property);
        }
    }
}
