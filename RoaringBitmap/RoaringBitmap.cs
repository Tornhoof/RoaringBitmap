using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Collections.Special
{
    public class RoaringBitmap : IEnumerable<int>, IEquatable<RoaringBitmap>
    {
        private readonly RoaringArray m_HighLowContainer;

        private RoaringBitmap(RoaringArray input)
        {
            m_HighLowContainer = input;
        }

        public long Cardinality => m_HighLowContainer.Cardinality;

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

        public static RoaringBitmap Create(params int[] values)
        {
            return Create(values.AsEnumerable());
        }

        public RoaringBitmap Optimize()
        {
            return new RoaringBitmap(RoaringArray.Optimize(m_HighLowContainer));
        }

        public static RoaringBitmap Create(IEnumerable<int> values)
        {
            var groupbyHb = values.Distinct().OrderBy(t => t).GroupBy(Util.HighBits).OrderBy(t => t.Key).ToList();
            var keys = new List<ushort>();
            var containers = new List<Container>();
            var size = 0;
            foreach (var group in groupbyHb)
            {
                keys.Add(group.Key);
                if (group.Count() > Container.MaxSize)
                {
                    containers.Add(BitmapContainer.Create(group.Select(Util.LowBits).ToArray()));
                }
                else
                {
                    containers.Add(ArrayContainer.Create(group.Select(Util.LowBits).ToArray()));
                }
                size++;
            }
            return new RoaringBitmap(new RoaringArray(size, keys, containers));
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

        public static RoaringBitmap AndNot(RoaringBitmap x, RoaringBitmap y)
        {
            return new RoaringBitmap(RoaringArray.AndNot(x.m_HighLowContainer, y.m_HighLowContainer));
        }

        public override bool Equals(object obj)
        {
            var ra = obj as RoaringArray;
            return (ra != null) && Equals(ra);
        }

        public override int GetHashCode()
        {
            return (13 ^ m_HighLowContainer.GetHashCode()) << 3;
        }

        public static void Serialize(RoaringBitmap roaringBitmap, Stream stream)
        {
            RoaringArray.Serialize(roaringBitmap.m_HighLowContainer, stream);
        }

        public static RoaringBitmap Deserialize(Stream stream)
        {
            var ra = RoaringArray.Deserialize(stream);
            return new RoaringBitmap(ra);
        }
    }
}