using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RoaringBitmap.Tests
{
    public class RoaringBitmapTests
    {
        [Fact]
        public void SmallArray()
        {
            var list = Enumerable.Range(0, 100).ToList();
            var rb = RoaringBitmap.Create(list);
            var rbList = rb.ToList();
            Assert.Equal(list, rbList);
        }

        [Fact]
        public void MaxArray()
        {
            var list = Enumerable.Range(0, Container.MaxSize + 1).ToList();
            var rb = RoaringBitmap.Create(list);
            var rbList = rb.ToList();
            Assert.Equal(list, rbList);
        }

        [Fact]
        public void LargeArray()
        {
            var list = CreateMixedListOne();
            var rb = RoaringBitmap.Create(list);
            var rbList = rb.ToList();
            Assert.Equal(list, rbList);
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

        [Fact]
        public void OrSame()
        {
            var list = CreateMixedListOne();
            var rb = RoaringBitmap.Create(list);
            var rb2 = rb | rb;
            Assert.NotNull(rb2);
            var rbList = rb2.ToList();
            Assert.Equal(list, rbList);
        }

        [Fact]
        public void OrDisjunct()
        {
            var firstList = CreateMixedListOne();
            var secondList = CreateMixedListTwo();
            var rb = RoaringBitmap.Create(firstList);
            var rb2 = RoaringBitmap.Create(secondList);
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
            var rb = RoaringBitmap.Create(firstList);
            lists.Add(firstList);
            var nextList = CreateRandomList(random, 10000);
            lists.Add(nextList);
            rb |= RoaringBitmap.Create(nextList);
            var comparison = lists.SelectMany(t => t).Distinct().OrderBy(t => t).ToList();
            var rbList = rb.ToList();
            Assert.Equal(comparison, rbList);
        }


        [Fact]
        public void AndRandom()
        {
            var random = new Random();
            var lists = new List<List<int>>();
            var firstList = CreateRandomList(random, 10000);
            var rb = RoaringBitmap.Create(firstList);
            lists.Add(firstList);
            for (var i = 0; i < 10; i++)
            {
                var nextList = CreateRandomList(random, 10000);
                lists.Add(nextList);
                rb &= RoaringBitmap.Create(nextList);
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
            var rb = RoaringBitmap.Create(list);
            var rb2 = rb & rb;
            Assert.NotNull(rb2);
            var rbList = rb2.ToList();
            Assert.Equal(list, rbList);
        }

        [Fact]
        public void AndDisjunct()
        {
            var firstList = CreateMixedListOne();
            var secondList = CreateMixedListTwo();
            var rb = RoaringBitmap.Create(firstList);
            var rb2 = RoaringBitmap.Create(secondList);
            var rb3 = rb & rb2;
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Empty(rbList);
            var comparison = firstList.Intersect(secondList).OrderBy(t => t).ToList();
            Assert.Equal(comparison, rbList);
        }

        [Fact]
        public void AndPartiallyArrayContainer()
        {
            var rb = RoaringBitmap.Create(Enumerable.Range(1000, 200));
            var rb2 = RoaringBitmap.Create(Enumerable.Range(1100, 400));
            var rb3 = rb & rb2;
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Equal(Enumerable.Range(1100, 100), rbList);
        }

        [Fact]
        public void AndPartiallyBitmapContainerArrayContainerResult()
        {
            var rb = RoaringBitmap.Create(Enumerable.Range(1000, 5000));
            var rb2 = RoaringBitmap.Create(Enumerable.Range(4000, 5000));
            var rb3 = rb & rb2;
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Equal(Enumerable.Range(4000, 2000), rbList);
        }

        [Fact]
        public void AndPartiallyBitmapContainerBitmapContainerResult()
        {
            var rb = RoaringBitmap.Create(Enumerable.Range(1000, 10000));
            var rb2 = RoaringBitmap.Create(Enumerable.Range(4000, 10000));
            var rb3 = rb & rb2;
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Equal(Enumerable.Range(4000, 7000), rbList);
        }

        [Fact]
        public void AndPartiallySameMixedContainer()
        {
            var rb = RoaringBitmap.Create(Enumerable.Range(1000, 4000));
            var rb2 = RoaringBitmap.Create(Enumerable.Range(4000, 10000));
            var rb3 = rb & rb2;
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Equal(Enumerable.Range(4000, 1000), rbList);
        }

        [Fact]
        public void Not()
        {
            var list = CreateMixedListOne();
            var rb = RoaringBitmap.Create(list);
            var rb2 = ~rb;
            var rb3 = ~rb2;

            var values = rb3.ToList();
            Assert.Equal(values, list);
        }

        [Fact]
        public void XorSame()
        {
            var list = CreateMixedListOne();
            var rb = RoaringBitmap.Create(list);
            var rb2 = rb ^ rb;
            var rbList = rb2.ToList();
            Assert.Empty(rbList);
        }

        [Fact]
        public void XorDisjunct()
        {
            var firstList = CreateMixedListOne();
            var secondList = CreateMixedListTwo();
            var rb = RoaringBitmap.Create(firstList);
            var rb2 = RoaringBitmap.Create(secondList);
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
            var rb = RoaringBitmap.Create(Enumerable.Range(1000, 200));
            var rb2 = RoaringBitmap.Create(Enumerable.Range(1100, 400));
            var rb3 = rb ^ rb2;
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Equal(Enumerable.Range(1000, 100).Concat(Enumerable.Range(1200, 300)), rbList);
        }


        [Fact]
        public void XorPartiallyBitmapContainerArrayContainerResult()
        {
            var rb = RoaringBitmap.Create(Enumerable.Range(1000, 5000));
            var rb2 = RoaringBitmap.Create(Enumerable.Range(4000, 5000));
            var rb3 = rb ^ rb2;
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Equal(Enumerable.Range(1000, 3000).Concat(Enumerable.Range(6000, 3000)), rbList);
        }

        [Fact]
        public void XorPartiallyBitmapContainerBitmapContainerResult()
        {
            var rb = RoaringBitmap.Create(Enumerable.Range(1000, 10000));
            var rb2 = RoaringBitmap.Create(Enumerable.Range(4000, 10000));
            var rb3 = rb ^ rb2;
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Equal(Enumerable.Range(1000, 3000).Concat(Enumerable.Range(11000, 3000)), rbList);
        }

        [Fact]
        public void XorPartiallySameMixedContainer()
        {
            var rb = RoaringBitmap.Create(Enumerable.Range(1000, 4000));
            var rb2 = RoaringBitmap.Create(Enumerable.Range(4000, 10000));
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
            var rb = RoaringBitmap.Create(firstList);
            var nextList = CreateRandomList(random, 10000);
            rb ^= RoaringBitmap.Create(nextList);
            var rbList = rb.ToList();
            var comparison = firstList.Union(nextList).Except(firstList.Intersect(nextList)).OrderBy(t => t).ToList();
            Assert.Equal(comparison, rbList);
        }

        [Fact]
        public void Equal()
        {
            var list = CreateMixedListOne();
            var rb = RoaringBitmap.Create(list);
            var rb2 = RoaringBitmap.Create(list);
            Assert.Equal(rb, rb2);
            Assert.Equal(rb.GetHashCode(), rb2.GetHashCode());
        }

        [Fact]
        public void NotEqual()
        {
            var rb = RoaringBitmap.Create(CreateMixedListOne());
            var rb2 = RoaringBitmap.Create(CreateMixedListTwo());
            Assert.NotEqual(rb, rb2);
            Assert.NotEqual(rb.GetHashCode(), rb2.GetHashCode());
        }

        [Fact]
        public void AndNotDisjunct()
        {
            var rb = RoaringBitmap.Create(CreateMixedListOne());
            var rb2 = RoaringBitmap.Create(CreateMixedListTwo());

            var rb3 = RoaringBitmap.AndNot(rb, rb2);
            Assert.NotNull(rb3);
            Assert.Equal(rb.ToList(), rb3.ToList());
        }

        [Fact]
        public void AndNotPartiallyArrayContainer()
        {
            var rb = RoaringBitmap.Create(Enumerable.Range(1000, 200));
            var rb2 = RoaringBitmap.Create(Enumerable.Range(1100, 400));
            var rb3 = RoaringBitmap.AndNot(rb, rb2);
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Equal(Enumerable.Range(1000, 100), rbList);
        }


        [Fact]
        public void AndNotPartiallyBitmapContainerArrayContainerResult()
        {
            var rb = RoaringBitmap.Create(Enumerable.Range(1000, 5000));
            var rb2 = RoaringBitmap.Create(Enumerable.Range(4000, 5000));
            var rb3 = RoaringBitmap.AndNot(rb, rb2);
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Equal(Enumerable.Range(1000, 3000), rbList);
        }

        [Fact]
        public void AndNotPartiallyBitmapContainerBitmapContainerResult()
        {
            var rb = RoaringBitmap.Create(Enumerable.Range(1000, 10000));
            var rb2 = RoaringBitmap.Create(Enumerable.Range(4000, 10000));
            var rb3 = RoaringBitmap.AndNot(rb, rb2);
            Assert.NotNull(rb3);
            var rbList = rb3.ToList();
            Assert.Equal(Enumerable.Range(1000, 3000), rbList);
        }

        [Fact]
        public void BasicCreate()
        {
            var rb = RoaringBitmap.Create(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
            var rb2 = RoaringBitmap.Create(Enumerable.Range(1, 10));
            Assert.Equal(rb, rb2);
        }
    }
}