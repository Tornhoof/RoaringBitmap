using System.Collections.Generic;

namespace Collections.Special
{
    internal interface IContainer
    {
        int Cardinality { get; }
        int ArraySizeInBytes { get; }
        bool EqualsInternal(IContainer other);
        IEnumerator<ushort> GetEnumerator();
    }
}