using System.IO;
using System.Linq;
using BenchmarkDotNet;

namespace RoaringBitmap.Benchmark.MicroBenchmarks
{
    public abstract class MicroBenchmark
    {
        private readonly RoaringBitmap[] m_Bitmaps;

        protected MicroBenchmark(string fileName)
        {
            var m_Path = @"Data";

            using (var provider = new ZipRealDataProvider(Path.Combine(m_Path, fileName)))
            {
                m_Bitmaps = provider.ToArray();
            }
        }

        [Benchmark]
        public int Or()
        {
            var total = 0;
            for (var k = 0; k < m_Bitmaps.Length - 1; k++)
            {
                total += (m_Bitmaps[k] | m_Bitmaps[k + 1]).Cardinality;
            }
            return total;
        }

        [Benchmark]
        public int Xor()
        {
            var total = 0;
            for (var k = 0; k < m_Bitmaps.Length - 1; k++)
            {
                total += (m_Bitmaps[k] ^ m_Bitmaps[k + 1]).Cardinality;
            }
            return total;
        }

        [Benchmark]
        public int And()
        {
            var total = 0;
            for (var k = 0; k < m_Bitmaps.Length - 1; k++)
            {
                total += (m_Bitmaps[k] & m_Bitmaps[k + 1]).Cardinality;
            }
            return total;
        }


        [Benchmark]
        public int Iterate()
        {
            var total = 0;
            foreach (var roaringBitmap in m_Bitmaps)
            {
                foreach (var @int in roaringBitmap)
                {
                    unchecked
                    {
                        total += @int;
                    }
                }
            }
            return total;
        }
    }
}