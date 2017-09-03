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

        [Params(1000, 10000, 100000)]
        public int Length { get; set; }

        [Params(3)]
        public int MaxDuplicates { get; set; }

        private IList<Entity> _outer;
        private IList<Entity> _inner;
        private IndexedCollection<Entity> _indexed;

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

            _indexed = _inner.IndexBy(e => e.Property);
        }

        [Benchmark]
        public int LinqJoin()
        {
            int sum = 0;

            _outer
                .Join(_inner,
                    o => o.Property, i => i.Property,
                    (o, i) => o.Property + i.Property)
                .ForEach(n => { sum += n; });

            return sum;
        }

        [Benchmark]
        public int IndexedJoin()
        {
            int sum = 0;

            _indexed
                .Join(_outer,
                    i => i.Property, o => o.Property,
                    (i, o) => i.Property + o.Property)
                .ForEach(n => { sum += n; });

            return sum;
        }
    }
}
