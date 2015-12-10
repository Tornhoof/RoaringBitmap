using System.Collections;
using System.Collections.Generic;

namespace RoaringBitmap
{
    public abstract class Container : IEnumerable<ushort>
    {
        public static int MaxSize = 4096; // everything <= is an ArrayContainer


        protected internal abstract int Cardinality { get; }

        public abstract IEnumerator<ushort> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public static Container operator |(Container x, Container y)
        {
            var xArrayContainer = x as ArrayContainer;
            var yArrayContainer = y as ArrayContainer;
            if (xArrayContainer != null && yArrayContainer != null)
            {
                return xArrayContainer | yArrayContainer;
            }
            if (xArrayContainer != null)
            {
                return xArrayContainer | (BitmapContainer) y;
            }
            if (yArrayContainer != null)
            {
                return (BitmapContainer) x | yArrayContainer;
            }
            return (BitmapContainer) x | (BitmapContainer) y;
        }

        public static Container operator &(Container x, Container y)
        {
            var xArrayContainer = x as ArrayContainer;
            var yArrayContainer = y as ArrayContainer;
            if (xArrayContainer != null && yArrayContainer != null)
            {
                return xArrayContainer & yArrayContainer;
            }
            if (xArrayContainer != null)
            {
                return xArrayContainer & (BitmapContainer) y;
            }
            if (yArrayContainer != null)
            {
                return (BitmapContainer) x & yArrayContainer;
            }
            return (BitmapContainer) x & (BitmapContainer) y;
        }

        public static Container operator ^(Container x, Container y)
        {
            var xArrayContainer = x as ArrayContainer;
            var yArrayContainer = y as ArrayContainer;
            if (xArrayContainer != null && yArrayContainer != null)
            {
                return xArrayContainer ^ yArrayContainer;
            }
            if (xArrayContainer != null)
            {
                return xArrayContainer ^ (BitmapContainer) y;
            }
            if (yArrayContainer != null)
            {
                return (BitmapContainer) x ^ yArrayContainer;
            }
            return (BitmapContainer) x ^ (BitmapContainer) y;
        }

        public static Container operator ~(Container x)
        {
            var xArrayContainer = x as ArrayContainer;
            return xArrayContainer != null ? ~xArrayContainer : ~(BitmapContainer) x;
        }
    }
}