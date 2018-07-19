using System;

namespace System.Collections {

    public interface IStructuralComparable {
        Int32 CompareTo(Object other, IComparer comparer);
    }
}
