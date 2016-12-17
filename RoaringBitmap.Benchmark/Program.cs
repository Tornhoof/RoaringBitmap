using System.Linq;
using BenchmarkDotNet.Running;
using RoaringBitmap.Benchmark.MicroBenchmarks;

namespace RoaringBitmap.Benchmark
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var types = typeof(MicroBenchmark).Assembly.GetTypes().Where(t => !t.IsAbstract && typeof(MicroBenchmark).IsAssignableFrom(t)).ToList();
            //var types = new[] { typeof(MicroBenchmarkCensusIncome) };
            foreach (var type in types)
            {
                BenchmarkRunner.Run(type);
            }
        }
    }
}