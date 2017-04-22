using System.Runtime.CompilerServices;

namespace Collections.Special
{
    internal static class Container
    {
        public const int MaxSize = 4096; // everything <= is an ArrayContainer
        public const int MaxCapacity = 1 << 16;

        public static bool Equals(IContainer x, IContainer y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }
            if (ReferenceEquals(null, y))
            {
                return false;
            }
            if (ReferenceEquals(null, x))
            {
                return false;
            }
            return x.EqualsInternal(y);
        }


        public static IContainer Or(ref IContainer x, ref IContainer y)
        {
            if (x is ArrayContainer xac && y is ArrayContainer yac)
            {
                return ArrayContainer.Or(ref xac, ref yac);
            }
            if (x is ArrayContainer xac2 && y is BitmapContainer ybc)
            {
                return BitmapContainer.Or(ref ybc, ref xac2);
            }
            if (x is BitmapContainer xbc && y is ArrayContainer yac2)
            {
                return BitmapContainer.Or(ref xbc, ref yac2);
            }
            xbc = (BitmapContainer) x;
            ybc = (BitmapContainer) y;
            return BitmapContainer.Or(ref xbc, ref ybc);
        }

        public static IContainer And(IContainer x, IContainer y)
        {
            if (x is ArrayContainer xac && y is ArrayContainer yac)
            {
                return ArrayContainer.And(xac, yac);
            }
            if (x is ArrayContainer xac2 && y is BitmapContainer ybc)
            {
                return ArrayContainer.And(xac2, ybc);
            }
            if (x is BitmapContainer xbc && y is ArrayContainer yac2)
            {
                return ArrayContainer.And(yac2, xbc);
            }
            return BitmapContainer.And((BitmapContainer) x, (BitmapContainer) y);
        }

        public static IContainer Xor(IContainer x, IContainer y)
        {
            if (x is ArrayContainer xac && y is ArrayContainer yac)
            {
                return ArrayContainer.Xor(xac, yac);
            }
            if (x is ArrayContainer xac2 && y is BitmapContainer ybc)
            {
                return BitmapContainer.Xor(ybc, xac2);
            }
            if (x is BitmapContainer xbc && y is ArrayContainer yac2)
            {
                return BitmapContainer.Xor(xbc, yac2);
            }
            return BitmapContainer.Xor((BitmapContainer) x, (BitmapContainer) y);
        }

        public static IContainer Not(IContainer x)
        {
            if (x is ArrayContainer ac)
            {
                return ArrayContainer.Not(ac);
            }
            return BitmapContainer.Not((BitmapContainer) x);
        }

        public static IContainer AndNot(IContainer x, IContainer y)
        {
            if (x is ArrayContainer xac && y is ArrayContainer yac)
            {
                return ArrayContainer.AndNot(xac, yac);
            }
            if (x is ArrayContainer xac2 && y is BitmapContainer ybc)
            {
                return ArrayContainer.AndNot(xac2, ybc);
            }
            if (x is BitmapContainer xbc && y is ArrayContainer yac2)
            {
                return BitmapContainer.AndNot(xbc, yac2);
            }
            return BitmapContainer.AndNot((BitmapContainer) x, (BitmapContainer) y);
        }
    }
}