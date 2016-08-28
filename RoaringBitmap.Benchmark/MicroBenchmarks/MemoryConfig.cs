using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnostics.Windows;

namespace RoaringBitmap.Benchmark.MicroBenchmarks
{
    public class MemoryConfig : ManualConfig
    {
        public MemoryConfig()
        {
            Add(new MemoryDiagnoser());
        }
    }
}