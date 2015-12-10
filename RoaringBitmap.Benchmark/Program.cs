using System;
using System.Linq;
using BenchmarkDotNet;
using RoaringBitmap.Benchmark.MicroBenchmarks;

namespace RoaringBitmap.Benchmark
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var types = typeof (MicroBenchmark).Assembly.GetTypes().Where(t => !t.IsAbstract && typeof (MicroBenchmark).IsAssignableFrom(t)).ToList();
            //var types = new[] { typeof(MicroBenchmarkCensusIncome) };
            var bRunner = new BenchmarkRunner();
            foreach (var type in types)
            {
                bRunner.RunCompetition(Activator.CreateInstance(type));
            }
        }
    }
}
