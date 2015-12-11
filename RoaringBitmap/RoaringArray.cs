using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RoaringBitmap
{
    public class RoaringArray : IEnumerable<int>, IEquatable<RoaringArray>
    {
        private readonly ushort[] m_Keys;
        private readonly int m_Size;
        private readonly Container[] m_Values;
        private readonly int m_Cardinality;

        // ReSharper disable once SuggestBaseTypeForParameter
        /// <summary>
        ///     Use List directly, because the enumerator is a struct
        /// </summary>
        internal RoaringArray(List<Tuple<ushort, Container>> containers)
        {
            m_Size = containers?.Count ?? 0;
            m_Keys = new ushort[m_Size];
            m_Values = new Container[m_Size];
            for (var i = 0; i < m_Size; i++)
            {
                Debug.Assert(containers != null);
                m_Keys[i] = containers[i].Item1;
                m_Values[i] = containers[i].Item2;
                m_Cardinality += m_Values[i].Cardinality;
            }
        }

        public int Cardinality => m_Cardinality;

        public IEnumerator<int> GetEnumerator()
        {
            for (var i = 0; i < m_Size; i++)
            {
                var key = m_Keys[i];
                var shiftedKey = key << 16;
                var container = m_Values[i];
                foreach (var @ushort in container)
                { 
                    yield return shiftedKey | @ushort;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Equals(RoaringArray other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (m_Size != other.m_Size)
            {
                return false;
            }
            if (m_Keys.Length != other.m_Keys.Length)
            {
                return false;
            }
            if (m_Values.Length != other.m_Values.Length)
            {
                return false;
            }
            for (var i = 0; i < m_Keys.Length; i++)
            {
                if (m_Keys[i] != other.m_Keys[i])
                {
                    return false;
                }
            }
            for (var i = 0; i < m_Values.Length; i++)
            {
                if (!m_Values[i].Equals(other.m_Values[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private int AdvanceUntil(ushort key, int index)
        {
            return Util.AdvanceUntil(m_Keys, index, m_Keys.Length, key);
        }

        public static RoaringArray operator |(RoaringArray x, RoaringArray y)
        {
            var xLength = x.m_Size;
            var yLength = y.m_Size;
            var list = new List<Tuple<ushort, Container>>(xLength + yLength);
            var xPos = 0;
            var yPos = 0;
            if (xPos < xLength && yPos < yLength)
            {
                var xKey = x.m_Keys[xPos];
                var yKey = y.m_Keys[yPos];
                while (true)
                {
                    if (xKey == yKey)
                    {
                        list.Add(new Tuple<ushort, Container>(xKey, x.m_Values[xPos] | y.m_Values[yPos]));
                        xPos++;
                        yPos++;
                        if ((xPos == xLength) || (yPos == yLength))
                        {
                            break;
                        }
                        xKey = x.m_Keys[xPos];
                        yKey = y.m_Keys[yPos];
                    }
                    else if (xKey < yKey)
                    {
                        list.Add(new Tuple<ushort, Container>(xKey, x.m_Values[xPos]));
                        xPos++;
                        if (xPos == xLength)
                        {
                            break;
                        }
                        xKey = x.m_Keys[xPos];
                    }
                    else
                    {
                        list.Add(new Tuple<ushort, Container>(yKey, y.m_Values[yPos]));
                        yPos++;
                        if (yPos == yLength)
                        {
                            break;
                        }
                        yKey = y.m_Keys[yPos];
                    }
                }
            }
            if (xPos == xLength)
            {
                for (var i = yPos; i < yLength; i++)
                {
                    list.Add(new Tuple<ushort, Container>(y.m_Keys[i], y.m_Values[i]));
                }
            }
            else if (yPos == yLength)
            {
                for (var i = xPos; i < xLength; i++)
                {
                    list.Add(new Tuple<ushort, Container>(x.m_Keys[i], x.m_Values[i]));
                }
            }
            return new RoaringArray(list);
        }

        public static RoaringArray operator &(RoaringArray x, RoaringArray y)
        {
            var xLength = x.m_Size;
            var yLength = y.m_Size;
            List<Tuple<ushort, Container>> list = null;
            var xPos = 0;
            var yPos = 0;
            while (xPos < xLength && yPos < yLength)
            {
                var xKey = x.m_Keys[xPos];
                var yKey = y.m_Keys[yPos];
                if (xKey == yKey)
                {
                    var c = x.m_Values[xPos] & y.m_Values[yPos];
                    if (c.Cardinality > 0)
                    {
                        if (list == null)
                        {
                            list = new List<Tuple<ushort, Container>>(Math.Min(xLength, yLength));
                        }
                        list.Add(new Tuple<ushort, Container>(xKey, c));
                    }
                    xPos++;
                    yPos++;
                }
                else if (xKey < yKey)
                {
                    xPos = x.AdvanceUntil(yKey, xPos);
                }
                else
                {
                    yPos = y.AdvanceUntil(xKey, yPos);
                }
            }
            return new RoaringArray(list);
        }

        public static RoaringArray operator ^(RoaringArray x, RoaringArray y)
        {
            var xLength = x.m_Size;
            var yLength = y.m_Size;
            var list = new List<Tuple<ushort, Container>>(xLength + yLength);
            var xPos = 0;
            var yPos = 0;
            if (xPos < xLength && yPos < yLength)
            {
                var xKey = x.m_Keys[xPos];
                var yKey = y.m_Keys[yPos];
                while (true)
                {
                    if (xKey == yKey)
                    {
                        list.Add(new Tuple<ushort, Container>(xKey, x.m_Values[xPos] ^ y.m_Values[yPos]));
                        xPos++;
                        yPos++;
                        if ((xPos == xLength) || (yPos == yLength))
                        {
                            break;
                        }
                        xKey = x.m_Keys[xPos];
                        yKey = y.m_Keys[yPos];
                    }
                    else if (xKey < yKey)
                    {
                        list.Add(new Tuple<ushort, Container>(xKey, x.m_Values[xPos]));
                        xPos++;
                        if (xPos == xLength)
                        {
                            break;
                        }
                        xKey = x.m_Keys[xPos];
                    }
                    else
                    {
                        list.Add(new Tuple<ushort, Container>(yKey, y.m_Values[yPos]));
                        yPos++;
                        if (yPos == yLength)
                        {
                            break;
                        }
                        yKey = y.m_Keys[yPos];
                    }
                }
            }
            if (xPos == xLength)
            {
                for (var i = yPos; i < yLength; i++)
                {
                    list.Add(new Tuple<ushort, Container>(y.m_Keys[i], y.m_Values[i]));
                }
            }
            else if (yPos == yLength)
            {
                for (var i = xPos; i < xLength; i++)
                {
                    list.Add(new Tuple<ushort, Container>(x.m_Keys[i], x.m_Values[i]));
                }
            }
            return new RoaringArray(list);
        }

        public static RoaringArray operator ~(RoaringArray x)
        {
            var list = new List<Tuple<ushort, Container>>(ushort.MaxValue);
            for (ushort i = 0; i < ushort.MaxValue; i++)
            {
                var index = Array.BinarySearch(x.m_Keys, i);
                if (index < 0)
                {
                    list.Add(new Tuple<ushort, Container>(i, BitmapContainer.One));
                }
                else
                {
                    var c = ~x.m_Values[index];
                    if (c.Cardinality > 0)
                    {
                        list.Add(new Tuple<ushort, Container>(i, c));
                    }
                }
            }
            return new RoaringArray(list);
        }

        public override bool Equals(object obj)
        {
            var ra = obj as RoaringArray;
            return ra != null && Equals(ra);
        }

        public override int GetHashCode()
        {
            var code = m_Size;
            code <<= 3;
            foreach (var i in m_Keys)
            {
                code ^= i;
                code <<= 3;
            }
            foreach (var c in m_Values)
            {
                code ^= c.GetHashCode();
                code <<= 3;
            }
            return code;
        }
    }
}