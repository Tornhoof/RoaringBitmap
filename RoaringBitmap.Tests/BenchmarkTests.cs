using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using RoaringBitmap.Benchmark;
using Xunit;
using Xunit.Abstractions;

namespace RoaringBitmap.Tests
{
    public class BenchmarkTests : IClassFixture<BenchmarkTests.BenchmarkTestsFixture>
    {
        private readonly BenchmarkTestsFixture m_Fixture;
        private readonly ITestOutputHelper m_OutputHelper;

        public BenchmarkTests(BenchmarkTestsFixture fixture, ITestOutputHelper outputHelper)
        {
            m_Fixture = fixture;
            m_OutputHelper = outputHelper;
        }

        [Theory]
        [InlineData(DataSets.CensusIncome, 12487395)]
        [InlineData(DataSets.Census1881, 2007691)]
        [InlineData(DataSets.Dimension003, 7733676)]
        [InlineData(DataSets.Dimension008, 5555233)]
        [InlineData(DataSets.Dimension033, 7579526)]
        [InlineData(DataSets.UsCensus2000, 11954)]
        [InlineData(DataSets.WeatherSept85, 24729002)]
        [InlineData(DataSets.WikileaksNoQuotes, 541893)]
        [InlineData(DataSets.CensusIncomeSrt, 11257282)]
        [InlineData(DataSets.Census1881Srt, 1360167)]
        [InlineData(DataSets.WeatherSept85Srt, 30863347)]
        [InlineData(DataSets.WikileaksNoQuotesSrt, 574463)]
        public void Or(string name, int value)
        {
            var bitmaps = m_Fixture.GetBitmaps(name);
            Assert.NotNull(bitmaps);
            Assert.Equal(value, OrInternal(bitmaps));
            Benchmark(OrInternal, bitmaps);
        }

        private void Benchmark(Func<RoaringBitmap[], int> functor, RoaringBitmap[] bitmaps)
        {
            var sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 10; i++)
            {
                functor(bitmaps);
            }
            sw.Stop();
            m_OutputHelper.WriteLine($"10 iterations: {sw.Elapsed}");
        }

        private int OrInternal(RoaringBitmap[] bitmaps)
        {
            var total = 0;
            for (var k = 0; k < bitmaps.Length - 1; k++)
            {
                total += (bitmaps[k] | bitmaps[k + 1]).Cardinality;
            }
            return total;
        }

        [Theory]
        [InlineData(DataSets.CensusIncome, 11241947)]
        [InlineData(DataSets.Census1881, 2007668)]
        [InlineData(DataSets.Dimension003, 7733676)]
        [InlineData(DataSets.Dimension008, 5442916)]
        [InlineData(DataSets.Dimension033, 7579526)]
        [InlineData(DataSets.UsCensus2000, 11954)]
        [InlineData(DataSets.WeatherSept85, 24086983)]
        [InlineData(DataSets.WikileaksNoQuotes, 538566)]
        [InlineData(DataSets.CensusIncomeSrt, 10329567)]
        [InlineData(DataSets.Census1881Srt, 1359961)]
        [InlineData(DataSets.WeatherSept85Srt, 29800358)]
        [InlineData(DataSets.WikileaksNoQuotesSrt, 574311)]
        public void Xor(string name, int value)
        {
            var bitmaps = m_Fixture.GetBitmaps(name);
            Assert.NotNull(bitmaps);
            Assert.Equal(value, XorInternal(bitmaps));
            Benchmark(XorInternal, bitmaps);
        }

        private int XorInternal(RoaringBitmap[] bitmaps)
        {
            var total = 0;
            for (var k = 0; k < bitmaps.Length - 1; k++)
            {
                total += (bitmaps[k] ^ bitmaps[k + 1]).Cardinality;
            }
            return total;
        }

        [Theory]
        [InlineData(DataSets.CensusIncome, 1245448)]
        [InlineData(DataSets.Census1881, 23)]
        [InlineData(DataSets.Dimension003, 0)]
        [InlineData(DataSets.Dimension008, 112317)]
        [InlineData(DataSets.Dimension033, 0)]
        [InlineData(DataSets.UsCensus2000, 0)]
        [InlineData(DataSets.WeatherSept85, 642019)]
        [InlineData(DataSets.WikileaksNoQuotes, 3327)]
        [InlineData(DataSets.CensusIncomeSrt, 927715)]
        [InlineData(DataSets.Census1881Srt, 206)]
        [InlineData(DataSets.WeatherSept85Srt, 1062989)]
        [InlineData(DataSets.WikileaksNoQuotesSrt, 152)]
        public void And(string name, int value)
        {
            var bitmaps = m_Fixture.GetBitmaps(name);
            Assert.NotNull(bitmaps);
            Assert.Equal(value, AndInternal(bitmaps));
            Benchmark(AndInternal, bitmaps);
        }

        private int AndInternal(RoaringBitmap[] bitmaps)
        {
            var total = 0;
            for (var k = 0; k < bitmaps.Length - 1; k++)
            {
                total += (bitmaps[k] & bitmaps[k + 1]).Cardinality;
            }
            return total;
        }

        [Theory]
        [InlineData(DataSets.CensusIncome, -942184551)]
        [InlineData(DataSets.Census1881, 246451066)]
        [InlineData(DataSets.Dimension003, -1287135055)]
        [InlineData(DataSets.Dimension008, -423436314)]
        [InlineData(DataSets.Dimension033, -1287135055)]
        [InlineData(DataSets.UsCensus2000, -1260727955)]
        [InlineData(DataSets.WeatherSept85, 644036874)]
        [InlineData(DataSets.WikileaksNoQuotes, 413846869)]
        [InlineData(DataSets.CensusIncomeSrt, -679313956)]
        [InlineData(DataSets.Census1881Srt, 445584405)]
        [InlineData(DataSets.WeatherSept85Srt, 1132748056)]
        [InlineData(DataSets.WikileaksNoQuotesSrt, 1921022163)]
        public void Iterate(string name, int value)
        {
            var bitmaps = m_Fixture.GetBitmaps(name);
            Assert.NotNull(bitmaps);
            var total = 0;
            foreach (var roaringBitmap in bitmaps)
            {
                foreach (var @int in roaringBitmap)
                {
                    unchecked
                    {
                        total += @int;
                    }
                }
            }
            Assert.Equal(value, total);
        }

        public class BenchmarkTestsFixture
        {
            private readonly Dictionary<string, RoaringBitmap[]> m_BitmapDictionary = new Dictionary<string, RoaringBitmap[]>();
            private readonly string m_Path = @"Data";

            public RoaringBitmap[] GetBitmaps(string name)
            {
                RoaringBitmap[] bitmaps;
                if (!m_BitmapDictionary.TryGetValue(name, out bitmaps))
                {
                    using (var provider = new ZipRealDataProvider(Path.Combine(m_Path, name)))
                    {
                        bitmaps = provider.ToArray();
                        m_BitmapDictionary[name] = bitmaps;
                    }
                }
                return bitmaps;
            }
        }
    }
}