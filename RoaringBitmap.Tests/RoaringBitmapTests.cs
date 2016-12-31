using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace RoaringBitmap.Tests
{
    public class RoaringBitmapTests
    {
        private static Stream GetResourceStream(string name)
        {
            return typeof(RoaringBitmapTests).Assembly.GetManifestResourceStream(typeof(RoaringBitmapTests), name);
        }

        private static List<int> CreateMixedListOne()
        {
            var list = new List<int>();
            var baseValue = 0x10000;
            for (var i = 0; i < 50; i++)
            {
                list.Add(i * 62);
            }

            for (var i = baseValue; i < baseValue + 100; i++)
            {
                list.Add(i);
            }
            for (var i = 2 * baseValue; i < 3 * baseValue; i++)
            {
                list.Add(i);
            }
            return list;
        }

        private static List<int> CreateMixedListTwo()
        {
            var list = new List<int>();
            var baseValue = 0x10000;
            for (var i = 1; i < 50; i++)
            {
                list.Add(i * 65);
            }

            for (var i = baseValue + 100; i < baseValue + 200; i++)
            {
                list.Add(i);
            }
            for (var i = 3 * baseValue; i < 4 * baseValue; i++)
            {
                list.Add(i);
            }
            return list;
        }

        private static List<int> CreateRandomList(Random random, int size)
        {
            var list = new List<int>();
            var type = random.Next() % 2;
            if (type == 0)
            {
                for (var i = 0; i < size; i++)
                {
                    list.Add(random.Next());
                }
            }
            else
            {
                var start = random.Next(0, int.MaxValue - size);
                for (var i = start; i < start + size; i++)
                {
                    list.Add(i);
                }
            }
            return list;
        }

        [Theory]
        [InlineData("bitmapwithoutruns.bin")]
        [InlineData("bitmapwithruns.bin")]
        public void DeSerializeTestContainers(string name)
        {
            using (var reference = GetResourceStream(name))
            {
                var items = new List<int>();
                for (var k = 0; k < 100000; k += 1000)
                {
                    items.Add(k);
                }
                for (var k = 100000; k < 200000; ++k)
                {
                    items.Add(3 * k);
                }
                for (var k = 700000; k < 800000; ++k)
                {
                    items.Add(k);
                }
                var rb = Collections.Special.RoaringBitmap.Create(items);
                var deserialized = Collections.Special.RoaringBitmap.Deserialize(reference);
                Assert.Equal(rb, deserialized);
            }
        }

        [Fact]
        public void AndDisjunct()
        {
            var firstList = CreateMixedListOne();
            var secondList = CreateMixedListTwo();
            var rb = Collections.Special.RoaringBitmap.Create(firstList);
            var rb2 = Collections.Special.RoaringBitmap.Create(secondList);
            var rb3 = rb & rb2;
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Empty(rbList);
            var comparison = firstList.Intersect(secondList).OrderBy(t => t).ToList();
            Assert.Equal(comparison, rbList);
        }

        [Fact]
        public void AndNotDisjunct()
        {
            var rb = Collections.Special.RoaringBitmap.Create(CreateMixedListOne());
            var rb2 = Collections.Special.RoaringBitmap.Create(CreateMixedListTwo());

            var rb3 = Collections.Special.RoaringBitmap.AndNot(rb, rb2);
            Assert.NotNull(rb3);
            Assert.Equal(rb.ToList(), rb3.ToList());
        }

        [Fact]
        public void AndNotPartiallyArrayContainer()
        {
            var rb = Collections.Special.RoaringBitmap.Create(Enumerable.Range(1000, 200));
            var rb2 = Collections.Special.RoaringBitmap.Create(Enumerable.Range(1100, 400));
            var rb3 = Collections.Special.RoaringBitmap.AndNot(rb, rb2);
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Equal(Enumerable.Range(1000, 100), rbList);
        }


        [Fact]
        public void AndNotPartiallyBitmapContainerArrayContainerResult()
        {
            var rb = Collections.Special.RoaringBitmap.Create(Enumerable.Range(1000, 5000));
            var rb2 = Collections.Special.RoaringBitmap.Create(Enumerable.Range(4000, 5000));
            var rb3 = Collections.Special.RoaringBitmap.AndNot(rb, rb2);
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Equal(Enumerable.Range(1000, 3000), rbList);
        }

        [Fact]
        public void AndNotPartiallyBitmapContainerBitmapContainerResult()
        {
            var rb = Collections.Special.RoaringBitmap.Create(Enumerable.Range(1000, 10000));
            var rb2 = Collections.Special.RoaringBitmap.Create(Enumerable.Range(4000, 10000));
            var rb3 = Collections.Special.RoaringBitmap.AndNot(rb, rb2);
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Equal(Enumerable.Range(1000, 3000), rbList);
        }

        [Fact]
        public void AndPartiallyArrayContainer()
        {
            var rb = Collections.Special.RoaringBitmap.Create(Enumerable.Range(1000, 200));
            var rb2 = Collections.Special.RoaringBitmap.Create(Enumerable.Range(1100, 400));
            var rb3 = rb & rb2;
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Equal(Enumerable.Range(1100, 100), rbList);
        }

        [Fact]
        public void AndPartiallyBitmapContainerArrayContainerResult()
        {
            var rb = Collections.Special.RoaringBitmap.Create(Enumerable.Range(1000, 5000));
            var rb2 = Collections.Special.RoaringBitmap.Create(Enumerable.Range(4000, 5000));
            var rb3 = rb & rb2;
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Equal(Enumerable.Range(4000, 2000), rbList);
        }

        [Fact]
        public void AndPartiallyBitmapContainerBitmapContainerResult()
        {
            var rb = Collections.Special.RoaringBitmap.Create(Enumerable.Range(1000, 10000));
            var rb2 = Collections.Special.RoaringBitmap.Create(Enumerable.Range(4000, 10000));
            var rb3 = rb & rb2;
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Equal(Enumerable.Range(4000, 7000), rbList);
        }

        [Fact]
        public void AndPartiallySameMixedContainer()
        {
            var rb = Collections.Special.RoaringBitmap.Create(Enumerable.Range(1000, 4000));
            var rb2 = Collections.Special.RoaringBitmap.Create(Enumerable.Range(4000, 10000));
            var rb3 = rb & rb2;
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Equal(Enumerable.Range(4000, 1000), rbList);
        }


        [Fact]
        public void AndRandom()
        {
            var random = new Random();
            var lists = new List<List<int>>();
            var firstList = CreateRandomList(random, 10000);
            var rb = Collections.Special.RoaringBitmap.Create(firstList);
            lists.Add(firstList);
            for (var i = 0; i < 10; i++)
            {
                var nextList = CreateRandomList(random, 10000);
                lists.Add(nextList);
                rb &= Collections.Special.RoaringBitmap.Create(nextList);
            }
            var comparison = lists.Skip(1).Aggregate(new HashSet<int>(lists.First()),
                                      (h, e) =>
                                      {
                                          h.IntersectWith(e);
                                          return h;
                                      });
            var rbList = rb.ToList();
            Assert.Equal(comparison, rbList);
        }

        [Fact]
        public void AndSame()
        {
            var list = CreateMixedListOne();
            var rb = Collections.Special.RoaringBitmap.Create(list);
            var rb2 = rb & rb;
            Assert.NotNull(rb2);
            var rbList = rb2.ToList();
            Assert.Equal(list, rbList);
        }

        [Fact]
        public void BasicCreate()
        {
            var rb = Collections.Special.RoaringBitmap.Create(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
            var rb2 = Collections.Special.RoaringBitmap.Create(Enumerable.Range(1, 10));
            Assert.Equal(rb, rb2);
        }

        [Fact]
        public void CardinalityOfEmptySet()
        {
            var full = Collections.Special.RoaringBitmap.Create();

            Assert.Equal(0, full.Cardinality);
        }

        [Fact]
        public void CardinalityOfFullSet()
        {
            var full = ~Collections.Special.RoaringBitmap.Create();

            Assert.Equal(1L << 32, full.Cardinality);
        }

        [Fact]
        public void Equal()
        {
            var list = CreateMixedListOne();
            var rb = Collections.Special.RoaringBitmap.Create(list);
            var rb2 = Collections.Special.RoaringBitmap.Create(list);
            Assert.Equal(rb, rb2);
            Assert.Equal(rb.GetHashCode(), rb2.GetHashCode());
        }

        [Fact]
        public void LargeArray()
        {
            var list = CreateMixedListOne();
            var rb = Collections.Special.RoaringBitmap.Create(list);
            var rbList = rb.ToList();
            Assert.Equal(list, rbList);
        }

        [Fact]
        public void MaxArray()
        {
            var list = Enumerable.Range(0, 4097).ToList();
            var rb = Collections.Special.RoaringBitmap.Create(list);
            var rbList = rb.ToList();
            Assert.Equal(list, rbList);
        }

        [Fact]
        public void Not()
        {
            var list = CreateMixedListOne();
            var rb = Collections.Special.RoaringBitmap.Create(list);
            var rb2 = ~rb;
            var rb3 = ~rb2;

            var values = rb3.ToList();
            Assert.Equal(values, list);
        }

        [Fact]
        public void NotEqual()
        {
            var rb = Collections.Special.RoaringBitmap.Create(CreateMixedListOne());
            var rb2 = Collections.Special.RoaringBitmap.Create(CreateMixedListTwo());
            Assert.NotEqual(rb, rb2);
            Assert.NotEqual(rb.GetHashCode(), rb2.GetHashCode());
        }

        [Fact]
        public void OptimizeFullSetArrayContainer()
        {
            var full = Collections.Special.RoaringBitmap.Create(Enumerable.Range(0, 4096));
            var fullOptimized = full.Optimize();
            Assert.NotNull(fullOptimized);
            Assert.False(ReferenceEquals(full, fullOptimized));
            Assert.Equal(Enumerable.Range(0, 4096), fullOptimized.ToList());
        }

        [Fact]
        public void OptimizeFullSetBitmapContainer()
        {
            var full = ~Collections.Special.RoaringBitmap.Create();
            var fullOptimized = full.Optimize();
            Assert.NotNull(fullOptimized);
            Assert.False(ReferenceEquals(full, fullOptimized));
            var empty = ~fullOptimized;
            var emptyList = empty.ToList();
            Assert.Empty(emptyList);
        }

        [Fact]
        public void OrDisjunct()
        {
            var firstList = CreateMixedListOne();
            var secondList = CreateMixedListTwo();
            var rb = Collections.Special.RoaringBitmap.Create(firstList);
            var rb2 = Collections.Special.RoaringBitmap.Create(secondList);
            var rb3 = rb | rb2;
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            var comparison = firstList.Union(secondList).OrderBy(t => t).ToList();
            Assert.Equal(comparison, rbList);
        }

        [Fact]
        public void OrRandom()
        {
            var random = new Random(4);
            var lists = new List<List<int>>();
            var firstList = CreateRandomList(random, 10000);
            var rb = Collections.Special.RoaringBitmap.Create(firstList);
            lists.Add(firstList);
            var nextList = CreateRandomList(random, 10000);
            lists.Add(nextList);
            rb |= Collections.Special.RoaringBitmap.Create(nextList);
            var comparison = lists.SelectMany(t => t).Distinct().OrderBy(t => t).ToList();
            var rbList = rb.ToList();
            Assert.Equal(comparison, rbList);
        }

        [Fact]
        public void OrSame()
        {
            var list = CreateMixedListOne();
            var rb = Collections.Special.RoaringBitmap.Create(list);
            var rb2 = rb | rb;
            Assert.NotNull(rb2);
            var rbList = rb2.ToList();
            Assert.Equal(list, rbList);
        }


        [Fact]
        public void SerializeDeserialize()
        {
            var rb = Collections.Special.RoaringBitmap.Create(Enumerable.Range(1, 100000));
            using (var ms = new MemoryStream())
            {
                Collections.Special.RoaringBitmap.Serialize(rb, ms);
                ms.Position = 0;
                var rb2 = Collections.Special.RoaringBitmap.Deserialize(ms);
                Assert.Equal(rb, rb2);
            }
        }


        [Fact]
        public void SerializeEmptySet()
        {
            var full = Collections.Special.RoaringBitmap.Create();

            using (var ms = new MemoryStream())
            {
                Collections.Special.RoaringBitmap.Serialize(full, ms);
                ms.Position = 0;
                var rb2 = Collections.Special.RoaringBitmap.Deserialize(ms);
                Assert.Equal(full, rb2);
            }
        }

        [Fact]
        public void SerializeFullArrayContainer()
        {
            var full = Collections.Special.RoaringBitmap.Create(Enumerable.Range(0, 4096));

            using (var ms = new MemoryStream())
            {
                Collections.Special.RoaringBitmap.Serialize(full, ms);
                ms.Position = 0;
                var rb2 = Collections.Special.RoaringBitmap.Deserialize(ms);
                Assert.Equal(full, rb2);
            }
        }

        [Fact]
        public void SerializeFullBitContainer()
        {
            var full = Collections.Special.RoaringBitmap.Create(Enumerable.Range(0, 1 << 16));

            using (var ms = new MemoryStream())
            {
                Collections.Special.RoaringBitmap.Serialize(full, ms);
                ms.Position = 0;
                var rb2 = Collections.Special.RoaringBitmap.Deserialize(ms);
                Assert.Equal(full, rb2);
            }
        }

        [Fact]
        public void SerializeFullSet()
        {
            var full = ~Collections.Special.RoaringBitmap.Create();

            using (var ms = new MemoryStream())
            {
                Collections.Special.RoaringBitmap.Serialize(full, ms);
                ms.Position = 0;
                var rb2 = Collections.Special.RoaringBitmap.Deserialize(ms);
                Assert.Equal(full, rb2);
            }
        }

        [Fact] // the testdata container https://github.com/RoaringBitmap/RoaringFormatSpec/tree/master/testdata
        public void SerializeTestContainer()
        {
            var items = new List<int>();
            for (var k = 0; k < 100000; k += 1000)
            {
                items.Add(k);
            }
            for (var k = 100000; k < 200000; ++k)
            {
                items.Add(3 * k);
            }
            for (var k = 700000; k < 800000; ++k)
            {
                items.Add(k);
            }
            var rb = Collections.Special.RoaringBitmap.Create(items);
            using (var ms = new MemoryStream())
            {
                Collections.Special.RoaringBitmap.Serialize(rb, ms);
                ms.Position = 0;
                var rb2 = Collections.Special.RoaringBitmap.Deserialize(ms);
                Assert.Equal(rb, rb2);
            }
        }

        [Fact]
        public void SmallArray()
        {
            var list = Enumerable.Range(0, 100).ToList();
            var rb = Collections.Special.RoaringBitmap.Create(list);
            var rbList = rb.ToList();
            Assert.Equal(list, rbList);
        }

        [Fact]
        public void XorDisjunct()
        {
            var firstList = CreateMixedListOne();
            var secondList = CreateMixedListTwo();
            var rb = Collections.Special.RoaringBitmap.Create(firstList);
            var rb2 = Collections.Special.RoaringBitmap.Create(secondList);
            var rb3 = rb ^ rb2;
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.NotEmpty(rbList);
            var comparison = firstList.Union(secondList).OrderBy(t => t).ToList();
            Assert.Equal(comparison, rbList);
        }


        [Fact]
        public void XorPartiallyArrayContainer()
        {
            var rb = Collections.Special.RoaringBitmap.Create(Enumerable.Range(1000, 200));
            var rb2 = Collections.Special.RoaringBitmap.Create(Enumerable.Range(1100, 400));
            var rb3 = rb ^ rb2;
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Equal(Enumerable.Range(1000, 100).Concat(Enumerable.Range(1200, 300)), rbList);
        }


        [Fact]
        public void XorPartiallyBitmapContainerArrayContainerResult()
        {
            var rb = Collections.Special.RoaringBitmap.Create(Enumerable.Range(1000, 5000));
            var rb2 = Collections.Special.RoaringBitmap.Create(Enumerable.Range(4000, 5000));
            var rb3 = rb ^ rb2;
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Equal(Enumerable.Range(1000, 3000).Concat(Enumerable.Range(6000, 3000)), rbList);
        }

        [Fact]
        public void XorPartiallyBitmapContainerBitmapContainerResult()
        {
            var rb = Collections.Special.RoaringBitmap.Create(Enumerable.Range(1000, 10000));
            var rb2 = Collections.Special.RoaringBitmap.Create(Enumerable.Range(4000, 10000));
            var rb3 = rb ^ rb2;
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Equal(Enumerable.Range(1000, 3000).Concat(Enumerable.Range(11000, 3000)), rbList);
        }

        [Fact]
        public void XorPartiallySameMixedContainer()
        {
            var rb = Collections.Special.RoaringBitmap.Create(Enumerable.Range(1000, 4000));
            var rb2 = Collections.Special.RoaringBitmap.Create(Enumerable.Range(4000, 10000));
            var rb3 = rb ^ rb2;
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Equal(Enumerable.Range(1000, 3000).Concat(Enumerable.Range(5000, 9000)), rbList);
        }

        [Fact]
        public void XorRandom()
        {
            var random = new Random();
            var firstList = CreateRandomList(random, 10000);
            var rb = Collections.Special.RoaringBitmap.Create(firstList);
            var nextList = CreateRandomList(random, 10000);
            rb ^= Collections.Special.RoaringBitmap.Create(nextList);
            var rbList = rb.ToList();
            var comparison = firstList.Union(nextList).Except(firstList.Intersect(nextList)).OrderBy(t => t).ToList();
            Assert.Equal(comparison, rbList);
        }

        [Fact]
        public void XorSame()
        {
            var list = CreateMixedListOne();
            var rb = Collections.Special.RoaringBitmap.Create(list);
            var rb2 = rb ^ rb;
            var rbList = rb2.ToList();
            Assert.Empty(rbList);
        }
    }
}