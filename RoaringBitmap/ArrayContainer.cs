using System;
using System.Collections.Generic;
using System.IO;

namespace Collections.Special
{
    internal struct ArrayContainer : IContainer, IEquatable<ArrayContainer>
    {
        public static readonly ArrayContainer One = InitializeOneContainer();
        private readonly int m_Cardinality;
        private readonly ushort[] m_Content;

        private static ArrayContainer InitializeOneContainer()
        {
            var data = new ushort[Container.MaxSize];
            for (ushort i = 0; i < Container.MaxSize; i++)
            {
                data[i] = i;
            }
            return new ArrayContainer(Container.MaxSize, data);
        }

        private ArrayContainer(int cardinality, ushort[] data)
        {
            m_Content = data;
            m_Cardinality = cardinality;
        }

        public int Cardinality => m_Cardinality;

        public int ArraySizeInBytes => m_Cardinality * sizeof(ushort);

        internal static ArrayContainer Create(ushort[] values)
        {
            return new ArrayContainer(values.Length, values);
        }

        internal static ArrayContainer Create(BitmapContainer bc)
        {
            var data = new ushort[bc.Cardinality];
            var cardinality = bc.FillArray(data);
            var result = new ArrayContainer(cardinality, data);
            return result;
        }

        public bool EqualsInternal(IContainer other)
        {
            return other is ArrayContainer && Equals((ArrayContainer) other);
        }

        public IEnumerator<ushort> GetEnumerator()
        {
            for (var i = 0; i < m_Cardinality; i++)
            {
                yield return m_Content[i];
            }
        }


        public static IContainer And(ArrayContainer x, ArrayContainer y)
        {
            var desiredCapacity = Math.Min(x.m_Cardinality, y.m_Cardinality);
            var data = new ushort[desiredCapacity];
            var calculatedCardinality = Util.IntersectArrays(x.m_Content, x.m_Cardinality, y.m_Content, y.m_Cardinality, data);
            return new ArrayContainer(calculatedCardinality, data);
        }

        public static ArrayContainer And(ArrayContainer x, BitmapContainer y)
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

        public static IContainer Or(ref ArrayContainer x, ref ArrayContainer y)
        {
            var totalCardinality = x.m_Cardinality + y.m_Cardinality;
            if (totalCardinality > Container.MaxSize)
            {
                var output = new ushort[totalCardinality];
                var calcCardinality = Util.UnionArrays(x.m_Content, x.m_Cardinality, y.m_Content, y.m_Cardinality, output);
                if (calcCardinality > Container.MaxSize)
                {
                    return BitmapContainer.Create(calcCardinality, output);
                }
                return new ArrayContainer(calcCardinality, output);
            }
            var desiredCapacity = totalCardinality;
            var data = new ushort[desiredCapacity];
            var calculatedCardinality = Util.UnionArrays(x.m_Content, x.m_Cardinality, y.m_Content, y.m_Cardinality, data);
            return new ArrayContainer(calculatedCardinality, data);
        }

        public static IContainer Not(ArrayContainer x)
        {
            return BitmapContainer.Create(x.m_Cardinality, x.m_Content, true); // an arraycontainer only contains up to 4096 values, so the negation is a bitmap IContainer
        }

        public static IContainer Xor(ArrayContainer x, ArrayContainer y)
        {
            var totalCardinality = x.m_Cardinality + y.m_Cardinality;
            if (totalCardinality > Container.MaxSize)
            {
                var bc = BitmapContainer.CreateXor(x.m_Content, x.Cardinality, y.m_Content, y.Cardinality);
                if (bc.Cardinality <= Container.MaxSize)
                {
                    Create(bc);
                }
            }
            var desiredCapacity = totalCardinality;
            var data = new ushort[desiredCapacity];
            var calculatedCardinality = Util.XorArrays(x.m_Content, x.m_Cardinality, y.m_Content, y.m_Cardinality, data);
            return new ArrayContainer(calculatedCardinality, data);
        }


        public static IContainer AndNot(ArrayContainer x, ArrayContainer y)
        {
            var desiredCapacity = x.m_Cardinality;
            var data = new ushort[desiredCapacity];
            var calculatedCardinality = Util.DifferenceArrays(x.m_Content, x.m_Cardinality, y.m_Content, y.m_Cardinality, data);
            return new ArrayContainer(calculatedCardinality, data);
        }

        public static IContainer AndNot(ArrayContainer x, BitmapContainer y)
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
                var mask = 1UL << yValue;
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
                var after = previous & ~(1UL << yValue);
                bitmap[index] = after;
                extraCardinality -= (int) ((previous ^ after) >> yValue);
            }
            return extraCardinality;
        }

        public bool Equals(ArrayContainer other)
        {
            if (m_Cardinality != other.m_Cardinality)
            {
                return false;
            }
            for (var i = 0; i < m_Cardinality; i++)
            {
                if (m_Content[i] != other.m_Content[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is ArrayContainer && Equals((ArrayContainer)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var code = 17;
                code = code * 23 + m_Cardinality;
                for (var i = 0; i < m_Cardinality; i++)
                {
                    code = code * 23 + m_Content[i];
                }
                return code;
            }
        }

        public static void Serialize(ArrayContainer ac, BinaryWriter binaryWriter)
        {
            for (var i = 0; i < ac.m_Cardinality; i++)
            {
                binaryWriter.Write(ac.m_Content[i]);
            }
        }

        public static ArrayContainer Deserialize(BinaryReader binaryReader, int cardinality)
        {
            var data = new ushort[cardinality];
            for (var i = 0; i < cardinality; i++)
            {
                data[i] = binaryReader.ReadUInt16();
            }
            return new ArrayContainer(cardinality, data);
        }
    }
}