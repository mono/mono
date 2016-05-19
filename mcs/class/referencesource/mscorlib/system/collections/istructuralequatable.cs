namespace System.Collections {

    public interface IStructuralEquatable {
        Boolean Equals(Object other, IEqualityComparer comparer);
        int GetHashCode(IEqualityComparer comparer);
    }
}