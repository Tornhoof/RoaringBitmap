using System;
using System.Collections.Generic;

namespace RoaringBitmap
{
    public class ArrayContainer : Container, IEquatable<ArrayContainer>
    {
        public static readonly ArrayContainer Zero;
        private readonly int m_Cardinality;
        private readonly ushort[] m_Content;

        static ArrayContainer()
        {
            var data = new ushort[MaxSize];
            for (ushort i = 0; i < MaxSize; i++)
            {
                data[i] = i;
            }
            Zero = new ArrayContainer(0);
        }

        private ArrayContainer(int cardinality)
        {
           m_Content = new ushort[cardinality];
            m_Cardinality = cardinality;
        }

        private ArrayContainer(int cardinality, Func<ushort[], int> functor)
        {
            m_Content = new ushort[cardinality];
            m_Cardinality = functor(m_Content);
        }

        private ArrayContainer(int cardinality, ushort[] data)
        {
            m_Content = data;
            m_Cardinality = cardinality;
        }

        protected internal override int Cardinality => m_Cardinality;


        public bool Equals(ArrayContainer other)
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
            if (m_Content.Length != other.m_Content.Length)
            {
                return false;
            }
            for (var i = 0; i < m_Content.Length; i++)
            {
                if (m_Content[i] != other.m_Content[i])
                {
                    return false;
                }
            }
            return true;
        }

        internal static ArrayContainer Create(ushort[] values)
        {
            return new ArrayContainer(values.Length, values);
        }

        internal static ArrayContainer Create(int cardinality, BitmapContainer bc)
        {
            return new ArrayContainer(cardinality, bc.FillArray);
        }

        public override IEnumerator<ushort> GetEnumerator()
        {
            for (var i = 0; i < m_Cardinality; i++)
            {
                yield return m_Content[i];
            }
        }

        public static Container operator &(ArrayContainer x, ArrayContainer y)
        {
            var desiredCapacity = Math.Min(x.m_Cardinality, y.m_Cardinality);
            return new ArrayContainer(desiredCapacity, buffer => Util.IntersectArrays(x.m_Content, x.m_Cardinality, y.m_Content, y.m_Cardinality, buffer));
        }

        public static ArrayContainer operator &(ArrayContainer x, BitmapContainer y)
        {
            var data = new ushort[x.m_Content.Length];
            var c = x.m_Cardinality;
            var pos = 0;
            for (var i = 0; i < c; i++)
            {
                var v = x.m_Content[i];
                if (y.Contains(v))
                {
                    data[pos++] = v;
                }
            }
            return new ArrayContainer(pos, data);
        }

        public static Container operator |(ArrayContainer x, ArrayContainer y)
        {
            var totalCardinality = x.m_Cardinality + y.m_Cardinality;
            if (totalCardinality > MaxSize)
            {
                var output = new ushort[totalCardinality];
                var calcCardinality = Util.UnionArrays(x.m_Content, x.m_Cardinality, y.m_Content, y.m_Cardinality, output);
                if (calcCardinality > MaxSize)
                {
                    return BitmapContainer.Create(calcCardinality, output);
                }
                return new ArrayContainer(calcCardinality, output);
            }
            var desiredCapacity = totalCardinality;
            return new ArrayContainer(desiredCapacity, buffer => Util.UnionArrays(x.m_Content, x.m_Cardinality, y.m_Content, y.m_Cardinality, buffer));
        }

        public static Container operator |(ArrayContainer x, BitmapContainer y)
        {
            return y | x;
        }

        public static Container operator ~(ArrayContainer x)
        {
            return BitmapContainer.Create(x.m_Cardinality, x.m_Content, true); // an arraycontainer only contains up to 4096 values, so the negation is a bitmap container
        }

        public static Container operator ^(ArrayContainer x, ArrayContainer y)
        {
            var totalCardinality = x.m_Cardinality + y.m_Cardinality;
            if (totalCardinality > MaxSize)
            {
                var bc = BitmapContainer.CreateXor(x.m_Content, x.Cardinality, y.m_Content, y.Cardinality);
                if (bc.Cardinality <= MaxSize)
                {
                    return new ArrayContainer(bc.Cardinality, bc.FillArray);
                }
            }
            var desiredCapacity = totalCardinality;
            return new ArrayContainer(desiredCapacity, buffer => Util.XorArrays(x.m_Content, x.m_Cardinality, y.m_Content, y.m_Cardinality, buffer));
        }

        public static Container operator ^(ArrayContainer x, BitmapContainer y)
        {
            return y ^ x;
        }

        public static Container AndNot(ArrayContainer x, ArrayContainer y)
        {
            var desiredCapacity = x.m_Cardinality;
            return new ArrayContainer(desiredCapacity, buffer => Util.DifferenceArrays(x.m_Content, x.m_Cardinality, y.m_Content, y.m_Cardinality, buffer));
        }

        public static Container AndNot(ArrayContainer x, BitmapContainer y)
        {
            var data = new ushort[x.m_Content.Length];
            var c = x.m_Cardinality;
            var pos = 0;
            for (var i = 0; i < c; i++)
            {
                var v = x.m_Content[i];
                if (!y.Contains(v))
                {
                    data[pos++] = v;
                }
            }
            return new ArrayContainer(pos, data);
        }

        public int OrArray(ulong[] bitmap)
        {
            var extraCardinality = 0;
            var yC = m_Cardinality;
            for (var i = 0; i < yC; i++)
            {
                var yValue = m_Content[i];
                var index = yValue >> 6;
                var previous = bitmap[index];
                var after = previous | (1UL << yValue);
                bitmap[index] = after;
                extraCardinality += (int) ((previous - after) >> 63);
            }
            return extraCardinality;
        }

        public int XorArray(ulong[] bitmap)
        {
            var extraCardinality = 0;
            var yC = m_Cardinality;
            for (var i = 0; i < yC; i++)
            {
                var yValue = m_Content[i];
                var index = yValue >> 6;
                var previous = bitmap[index];
                var mask = (1UL << yValue);
                bitmap[index] = previous ^ mask;
                extraCardinality += (int) (1 - 2 * ((previous & mask) >> yValue));
            }
            return extraCardinality;
        }


        public int AndNotArray(ulong[] bitmap)
        {
            var extraCardinality = 0;
            var yC = m_Cardinality;
            for (var i = 0; i < yC; i++)
            {
                var yValue = m_Content[i];
                var index = yValue >> 6;
                var previous = bitmap[index];
                var after = previous & (~(1UL << yValue));
                bitmap[index] = after;
                extraCardinality -= (int) (((previous ^ after) >> yValue));
            }
            return extraCardinality;
        }

        public override bool Equals(object obj)
        {
            var ac = obj as ArrayContainer;
            return ac != null && Equals(ac);
        }

        public override int GetHashCode()
        {
            var code = m_Cardinality;
            code <<= 3;
            foreach (var @ushort in m_Content)
            {
                code ^= @ushort;
                code <<= 3;
            }
            return code;
        }
    }
}