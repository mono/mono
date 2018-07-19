namespace System.Web.UI.WebControls {
    using System.Collections;
    using System.Collections.Generic;

    internal class LinqDataSourceHelper {

        public static bool EnumerableContentEquals(IEnumerable enumerableA, IEnumerable enumerableB) {
            IEnumerator enumeratorA = enumerableA.GetEnumerator();
            IEnumerator enumeratorB = enumerableB.GetEnumerator();
            while (enumeratorA.MoveNext()) {
                if (!enumeratorB.MoveNext())
                    return false;
                object itemA = enumeratorA.Current;
                object itemB = enumeratorB.Current;
                if (itemA == null) {
                    if (itemB != null)
                        return false;
                }
                else if (!itemA.Equals(itemB))
                    return false;
            }
            if (enumeratorB.MoveNext())
                return false;
            return true;
        }

        public static Type FindGenericEnumerableType(Type type) {
            // Logic taken from Queryable.AsQueryable which accounts for Array types which are not
            // generic but implement the generic IEnumerable interface.
            while ((type != null) && (type != typeof(object)) && (type != typeof(string))) {
                if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IEnumerable<>))) {
                    return type;
                }
                foreach (Type interfaceType in type.GetInterfaces()) {
                    Type genericInterface = FindGenericEnumerableType(interfaceType);
                    if (genericInterface != null) {
                        return genericInterface;
                    }
                }
                type = type.BaseType;
            }
            return null;
        }

    }

}
