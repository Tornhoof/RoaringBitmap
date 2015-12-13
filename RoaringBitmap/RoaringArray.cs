using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RoaringBitmap
{
    public class RoaringArray : IEnumerable<int>, IEquatable<RoaringArray>
    {
        private readonly ushort[] m_Keys;
        private readonly int m_Size;
        private readonly Container[] m_Values;

        // ReSharper disable once SuggestBaseTypeForParameter
        /// <summary>
        ///     Use List directly, because the enumerator is a struct
        /// </summary>
        internal RoaringArray(int size, List<ushort> keys, List<Container> containers)
        {
            m_Size = size;
            m_Keys = new ushort[m_Size];
            m_Values = new Container[m_Size];
            for (var i = 0; i < m_Size; i++)
            {
                m_Keys[i] = keys[i];
                m_Values[i] = containers[i];
                Cardinality += m_Values[i].Cardinality;
            }
        }

        private RoaringArray(int size, ushort[] keys, Container[] containers)
        {
            m_Size = size;
            m_Keys = keys;
            m_Values = containers;
            Cardinality = m_Values.Sum(t => t.Cardinality);
        }

        public int Cardinality { get; }

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
            var keys = new List<ushort>(xLength + yLength);
            var containers = new List<Container>(xLength + yLength);
            var size = 0;
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
                        keys.Add(xKey);
                        containers.Add(x.m_Values[xPos] | y.m_Values[yPos]);
                        size++;
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
                        keys.Add(xKey);
                        containers.Add(x.m_Values[xPos]);
                        size++;
                        xPos++;
                        if (xPos == xLength)
                        {
                            break;
                        }
                        xKey = x.m_Keys[xPos];
                    }
                    else
                    {
                        keys.Add(yKey);
                        containers.Add(y.m_Values[yPos]);
                        size++;
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
                    keys.Add(y.m_Keys[i]);
                    containers.Add(y.m_Values[i]);
                    size++;
                }
            }
            else if (yPos == yLength)
            {
                for (var i = xPos; i < xLength; i++)
                {
                    keys.Add(x.m_Keys[i]);
                    containers.Add(x.m_Values[i]);
                    size++;
                }
            }
            return new RoaringArray(size, keys, containers);
        }

        public static RoaringArray operator &(RoaringArray x, RoaringArray y)
        {
            var xLength = x.m_Size;
            var yLength = y.m_Size;
            List<ushort> keys = null;
            List<Container> containers = null;
            var size = 0;
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
                        if (keys == null)
                        {
                            var length = Math.Min(xLength, yLength);
                            keys = new List<ushort>(length);
                            containers = new List<Container>(length);
                        }
                        keys.Add(xKey);
                        containers.Add(c);
                        size++;
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
            return new RoaringArray(size, keys, containers);
        }

        public static RoaringArray operator ^(RoaringArray x, RoaringArray y)
        {
            var xLength = x.m_Size;
            var yLength = y.m_Size;
            var keys = new List<ushort>(xLength + yLength);
            var containers = new List<Container>(xLength + yLength);
            var size = 0;
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
                        keys.Add(xKey);
                        containers.Add(x.m_Values[xPos] ^ y.m_Values[yPos]);
                        size++;
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
                        keys.Add(xKey);
                        containers.Add(x.m_Values[xPos]);
                        size++;
                        xPos++;
                        if (xPos == xLength)
                        {
                            break;
                        }
                        xKey = x.m_Keys[xPos];
                    }
                    else
                    {
                        keys.Add(yKey);
                        containers.Add(y.m_Values[yPos]);
                        size++;
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
                    keys.Add(y.m_Keys[i]);
                    containers.Add(y.m_Values[i]);
                    size++;
                }
            }
            else if (yPos == yLength)
            {
                for (var i = xPos; i < xLength; i++)
                {
                    keys.Add(x.m_Keys[i]);
                    containers.Add(x.m_Values[i]);
                    size++;
                }
            }
            return new RoaringArray(size, keys, containers);
        }

        public static RoaringArray operator ~(RoaringArray x)
        {
            var keys = new List<ushort>(ushort.MaxValue);
            var size = 0;
            var containers = new List<Container>(ushort.MaxValue);
            var oldIndex = 0;
            for (ushort i = 0; i < ushort.MaxValue; i++)
            {
                var index = Array.BinarySearch(x.m_Keys, oldIndex, x.m_Size - oldIndex, i);
                if (index < 0)
                {
                    keys.Add(i);
                    containers.Add(BitmapContainer.One);
                    size++;
                }
                else
                {
                    var c = x.m_Values[index];
                    if (!c.Equals(BitmapContainer.One)) // the bitwise negation of the one container is the zero container
                    {
                        var nc = ~c;
                        if (nc.Cardinality > 0)
                        {
                            keys.Add(i);
                            containers.Add(nc);
                            size++;
                        }
                    }
                    oldIndex = index;
                }
            }
            return new RoaringArray(size, keys, containers);
        }

        public static RoaringArray AndNot(RoaringArray x, RoaringArray y)
        {
            var xLength = x.m_Size;
            var yLength = y.m_Size;
            var keys = new List<ushort>(xLength);
            var containers = new List<Container>(xLength);
            var size = 0;
            var xPos = 0;
            var yPos = 0;
            while (xPos < xLength && yPos < yLength)
            {
                var xKey = x.m_Keys[xPos];
                var yKey = y.m_Keys[yPos];
                if (xKey == yKey)
                {
                    var c = Container.AndNot(x.m_Values[xPos], y.m_Values[yPos]);
                    if (c.Cardinality > 0)
                    {
                        keys.Add(xKey);
                        containers.Add(c);
                        size++;
                    }
                    xPos++;
                    yPos++;
                }
                else if (xKey < yKey)
                {
                    var next = x.AdvanceUntil(yKey, xPos);
                    for (var i = xPos; i < next; i++)
                    {
                        keys.Add(x.m_Keys[i]);
                        containers.Add(x.m_Values[i]);
                        size++;
                    }
                    xPos = next;
                }
                else
                {
                    yPos = y.AdvanceUntil(xKey, yPos);
                }
            }
            if (yPos == yLength)
            {
                for (var i = xPos; i < xLength; i++)
                {
                    keys.Add(x.m_Keys[i]);
                    containers.Add(x.m_Values[i]);
                    size++;
                }
            }
            return new RoaringArray(size, keys, containers);
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

        public static void Serialize(RoaringArray roaringArray, Stream stream)
        {
            using (var binaryWriter = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                binaryWriter.Write(roaringArray.m_Size);
                for (var i = 0; i < roaringArray.m_Keys.Length; i++)
                {
                    binaryWriter.Write(roaringArray.m_Keys[i]);
                    var c = roaringArray.m_Values[i];

                    if (Equals(c, ArrayContainer.One))
                    {
                        binaryWriter.Write((byte) 0);
                    }
                    else if (c is ArrayContainer)
                    {
                        binaryWriter.Write((byte) 1);
                        ArrayContainer.Serialize((ArrayContainer) c, binaryWriter);
                    }
                    else if (Equals(c, BitmapContainer.One))
                    {
                        binaryWriter.Write((byte) 0x7F);
                    }
                    else if (c is BitmapContainer)
                    {
                        binaryWriter.Write((byte) 0x80);
                        BitmapContainer.Serialize((BitmapContainer) c, binaryWriter);
                    }
                }
                binaryWriter.Flush();
            }
        }

        public static RoaringArray Deserialize(Stream stream)
        {
            using (var binaryReader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                var size = binaryReader.ReadInt32();
                var keys = new ushort[size];
                var containers = new Container[size];
                for (var i = 0; i < size; i++)
                {
                    keys[i] = binaryReader.ReadUInt16();
                    var type = binaryReader.ReadByte();
                    if (type == 0)
                    {
                        containers[i] = ArrayContainer.One;
                    }
                    else if (type == 1)
                    {
                        containers[i] = ArrayContainer.Deserialize(binaryReader);
                    }
                    else if (type == 0x7F)
                    {
                        containers[i] = BitmapContainer.One;
                    }
                    else if (type == 0x80)
                    {
                        containers[i] = BitmapContainer.Deserialize(binaryReader);
                    }
                }
                return new RoaringArray(size, keys, containers);
            }
        }
    }
}