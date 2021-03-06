﻿using System;
using System.Collections.Generic;
using BenchmarkDotNet.Running;

namespace MultiIndexCollection.Benchmarks
{
    static class Program
    {
        static void Main(string[] args)
        {
            new BenchmarkSwitcher(new[] {
                typeof(FilteringBenchmark),
                typeof(IndexingBenchmark),
                typeof(UpdatingBenchmark),
                typeof(LinqMethodsBenchmark),
            }).Run(args);
        }

        static readonly Random Random = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static T Enumerate<T>(this IEnumerable<T> enumerable)
        {
            T last = default(T);
            foreach (T item in enumerable)
            {
                last = item;
            }
            return last;
        }
    }
}
