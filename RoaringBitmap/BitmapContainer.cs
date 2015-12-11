using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RoaringBitmap
{
    public class BitmapContainer : Container, IEquatable<BitmapContainer>
    {
        public static readonly BitmapContainer One;
        private readonly ulong[] m_Bitmap;
        private readonly int m_Cardinality;

        static BitmapContainer()
        {
            var data = new ulong[1024];
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = ulong.MaxValue;
            }
            One = new BitmapContainer(1 << 16, data);
        }

        private BitmapContainer(int cardinality)
        {
            m_Bitmap = new ulong[1024];
            m_Cardinality = cardinality;
        }

        private BitmapContainer(int cardinality, ulong[] data)
        {
            m_Bitmap = data;
            m_Cardinality = cardinality;
        }

        private BitmapContainer(int cardinality, ushort[] values, bool negated) : this(negated ? ushort.MaxValue - cardinality +1 : cardinality)
        {
            if (negated)
            {
                for (var i = 0; i < m_Bitmap.Length; i++)
                {
                    m_Bitmap[i] = ulong.MaxValue;
                }
                for (var i = 0; i < cardinality; i++)
                {
                    var v = values[i];
                    m_Bitmap[v >> 6] &= ~(1UL << v);
                }
            }
            else
            {
                for (var i = 0; i < cardinality; i++)
                {
                    var v = values[i];
                    m_Bitmap[v >> 6] |= (1UL << v);
                }
            }
        }

        protected internal override int Cardinality => m_Cardinality;

        public bool Equals(BitmapContainer other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (m_Cardinality != other.m_Cardinality)
            {
                return false;
            }
            if (m_Bitmap.Length != other.m_Bitmap.Length)
            {
                return false;
            }
            for (var i = 0; i < m_Bitmap.Length; i++)
            {
                if (m_Bitmap[i] != other.m_Bitmap[i])
                {
                    return false;
                }
            }
            return true;
        }


        internal static BitmapContainer Create(ushort[] values)
        {
            return new BitmapContainer(values.Length, values, false);
        }

        internal static BitmapContainer Create(int cardinality, ushort[] values)
        {
            return new BitmapContainer(cardinality, values, false);
        }

        internal static BitmapContainer Create(int cardinality, ushort[] values, bool negated)
        {
            return new BitmapContainer(cardinality, values, negated);
        }


        internal static BitmapContainer CreateXor(ushort[] first, int firstCardinality, ushort[] second, int secondCardinality)
        {
            var data = new ulong[1024];
            for (var i = 0; i < firstCardinality; i++)
            {
                var v = first[i];
                data[v >> 6] ^= (1UL << v);
            }

            for (var i = 0; i < secondCardinality; i++)
            {
                var v = second[i];
                data[v >> 6] ^= (1UL << v);
            }
            var cardinality = 0;
            for (var i = 0; i < data.Length; i++)
            {
                cardinality += Util.BitCount(data[i]);
            }
            return new BitmapContainer(cardinality, data);
        }

        /// <summary>
        /// Java version has an optimized version of this, but it's using bitcount internally which should make it slower in .NET
        /// </summary>
        public static Container operator &(BitmapContainer x, BitmapContainer y)
        {
            var data = Clone(x.m_Bitmap);
            var bc = new BitmapContainer(AndInternal(data, y.m_Bitmap), data);
            return bc.m_Cardinality <= MaxSize ? (Container)ArrayContainer.Create(bc.Cardinality, bc) : bc;
        }

        private static ulong[] Clone(ulong[] data)
        {
            var result = new ulong[data.Length];
            Buffer.BlockCopy(data, 0, result, 0, data.Length*sizeof(ulong));
            return result;
        }

        public static ArrayContainer operator &(BitmapContainer x, ArrayContainer y)
        {
            return y & x;
        }

        public static BitmapContainer operator |(BitmapContainer x, BitmapContainer y)
        {
            var data = Clone(x.m_Bitmap);
            return new BitmapContainer(OrInternal(data, y.m_Bitmap), data);
        }

        public static BitmapContainer operator |(BitmapContainer x, ArrayContainer y)
        {
            var data = Clone(x.m_Bitmap);
            return new BitmapContainer(x.m_Cardinality + y.OrArray(data), data);
        }

        public static Container operator ~(BitmapContainer x)
        {
            var data = Clone(x.m_Bitmap);
            var bc = new BitmapContainer(NotInternal(data), data);
            return bc.m_Cardinality <= MaxSize ? (Container) ArrayContainer.Create(bc.Cardinality, bc) : bc;
        }

        /// <summary>
        /// Java version has an optimized version of this, but it's using bitcount internally which should make it slower in .NET
        /// </summary>
        public static Container operator ^(BitmapContainer x, BitmapContainer y)
        {
            var data = Clone(x.m_Bitmap);
            var bc = new BitmapContainer(XorInternal(data, y.m_Bitmap), data);
            return bc.m_Cardinality <= MaxSize ? (Container) ArrayContainer.Create(bc.Cardinality, bc) : bc;
        }


        public static Container operator ^(BitmapContainer x, ArrayContainer y)
        {
            var data = Clone(x.m_Bitmap);
            var bc = new BitmapContainer(x.m_Cardinality + y.XorArray(data), data);
            return bc.m_Cardinality <= MaxSize ? (Container) ArrayContainer.Create(bc.Cardinality, bc) : bc;
        }

        private static int XorInternal(ulong[] first, ulong[] second)
        {
            var c = 0;
            for (var k = 0; k < first.Length; k++)
            {
                var w = first[k] ^ second[k];
                first[k] = w;
                c += Util.BitCount(w);
            }
            return c;
        }

        private static int NotInternal(ulong[] data)
        {
            var c = 0;
            for (var k = 0; k < data.Length; k++)
            {
                var w = ~data[k];
                data[k] = w;
                c += Util.BitCount(w);
            }
            return c;
        }

        private static int OrInternal(ulong[] first, ulong[] second)
        {
            var c = 0;
            for (var k = 0; k < first.Length; k++)
            {
                var w = first[k] | second[k];
                first[k] = w;
                c += Util.BitCount(w);
            }
            return c;
        }

        private static int AndInternal(ulong[] first, ulong[] second)
        {
            var c = 0;
            for (var k = 0; k < first.Length; k++)
            {
                var w = first[k] & second[k];
                first[k] = w;
                c += Util.BitCount(w);
            }
            return c;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(ushort x)
        {
            return Contains(m_Bitmap, x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Contains(ulong[] bitmap, ushort x)
        {
            return (bitmap[x >> 6] & (1UL << x)) != 0;
        }

        public override IEnumerator<ushort> GetEnumerator()
        {
            for (var k = 0; k < m_Bitmap.Length; k++)
            {
                var bitset = m_Bitmap[k];
                var shiftedK = k << 6;
                while (bitset != 0)
                {
                    var t = bitset & (~bitset + 1);
                    var result = (ushort) (shiftedK + Util.BitCount(t - 1));
                    yield return result;
                    bitset ^= t;
                }
            }
        }

        internal int FillArray(ushort[] data)
        {
            var pos = 0;
            for (var k = 0; k < m_Bitmap.Length; k++)
            {
                var bitset = m_Bitmap[k];
                var shiftedK = k << 6;
                while (bitset != 0)
                {
                    var t = bitset & (~bitset + 1);
                    data[pos++] = (ushort)(shiftedK + Util.BitCount(t - 1));
                    bitset ^= t;
                }
            }
            return m_Cardinality;
        }

        public override bool Equals(object obj)
        {
            var bc = obj as BitmapContainer;
            return bc != null && Equals(bc);
        }

        public override int GetHashCode()
        {
            var code = (ulong)m_Cardinality;
            code <<= 3;
            foreach (var @ulong in m_Bitmap)
            {
                code ^= @ulong;
                code <<= 3;
            }
            return (int)((code & 0xFFFFFFFF) >> 32);
        }
    }
}