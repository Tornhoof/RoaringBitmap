using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RoaringBitmap
{
    public class RoaringBitmap : IEnumerable<int>, IEquatable<RoaringBitmap>
    {
        private readonly RoaringArray m_HighLowContainer;

        private RoaringBitmap(RoaringArray input)
        {
            m_HighLowContainer = input;
        }

        public int Cardinality => m_HighLowContainer.Cardinality;

        public IEnumerator<int> GetEnumerator()
        {
            return m_HighLowContainer.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Equals(RoaringBitmap other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            return m_HighLowContainer.Equals(other.m_HighLowContainer);
        }

        public static RoaringBitmap Create(IEnumerable<int> values)
        {
            var groupbyHb = values.Distinct().OrderBy(t => t).GroupBy(Util.HighBits).OrderBy(t => t.Key).ToList();
            var list = new List<Tuple<ushort, Container>>();
            foreach (var group in groupbyHb)
            {
                list.Add(@group.Count() > Container.MaxSize
                             ? new Tuple<ushort, Container>(@group.Key, BitmapContainer.Create(@group.Select(Util.LowBits).ToArray()))
                             : new Tuple<ushort, Container>(@group.Key, ArrayContainer.Create(@group.Select(Util.LowBits).ToArray())));
            }
            return new RoaringBitmap(new RoaringArray(list));
        }

        public static RoaringBitmap operator |(RoaringBitmap x, RoaringBitmap y)
        {
            return new RoaringBitmap(x.m_HighLowContainer | y.m_HighLowContainer);
        }

        public static RoaringBitmap operator &(RoaringBitmap x, RoaringBitmap y)
        {
            return new RoaringBitmap(x.m_HighLowContainer & y.m_HighLowContainer);
        }

        public static RoaringBitmap operator ~(RoaringBitmap x)
        {
            return new RoaringBitmap(~x.m_HighLowContainer);
        }


        public static RoaringBitmap operator ^(RoaringBitmap x, RoaringBitmap y)
        {
            return new RoaringBitmap(x.m_HighLowContainer ^ y.m_HighLowContainer);
        }

        public override bool Equals(object obj)
        {
            var ra = obj as RoaringArray;
            return ra != null && Equals(ra);
        }

        public override int GetHashCode()
        {
            return (13 ^ m_HighLowContainer.GetHashCode()) << 3;
        }
    }
}