using BenchmarkDotNet.Running;

namespace MultiIndexCollection.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            new BenchmarkSwitcher(new[] {
                typeof(FilteringBenchmark),
                typeof(IndexingBenchmark),
                typeof(UpdatingBenchmark),
            }).Run(args);
        }
    }
}
