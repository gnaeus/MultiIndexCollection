using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace MultiIndexCollection.Benchmarks
{
    // [MemoryDiagnoser]
    public class IndexingBenchmark
    {
        public class Product
        {
            public int Id { get; set; }
            public int Code { get; set; }
        }

        [Params(100, 300, 1000, 3000, 10000, 30000, 100000, 300000, 1000000)]
        public int Length { get; set; } = 10000;

        [Params(10, 30)]
        public int Duplicates { get; set; } = 10;
        
        private List<Product> _list;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var random = new Random();

            _list = new List<Product>(Length);

            int id = 0;
            for (int i = 0; i < Length && _list.Count < Length; i++)
            {
                for (int j = 0; j < Duplicates && _list.Count < Length; j++)
                {
                    _list.Add(new Product
                    {
                        Id = id++,
                        Code = i,
                    });
                }
            }

            _list.Shuffle();
        }

        [Benchmark]
        public object UniqueToLookup()
        {
            return _list.ToLookup(p => p.Id);
        }

        [Benchmark]
        public object UniqueHashIndex()
        {
            return _list.IndexBy(p => p.Id);
        }

        [Benchmark]
        public object UniqueSortedIndex()
        {
            return _list.IndexBy(p => p.Id, true);
        }

        [Benchmark]
        public object NonUniqueToLookup()
        {
            return _list.ToLookup(p => p.Code);
        }

        [Benchmark]
        public object NonUniqueHashIndex()
        {
            return _list.IndexBy(p => p.Code);
        }

        [Benchmark]
        public object NonUniqueSortedIndex()
        {
            return _list.IndexBy(p => p.Code, true);
        }
    }
}
