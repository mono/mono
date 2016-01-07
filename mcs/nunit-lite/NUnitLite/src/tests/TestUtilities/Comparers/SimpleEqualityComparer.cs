#if CLR_2_0 || CLR_4_0
using System.Collections;
using System.Collections.Generic;

namespace NUnit.TestUtilities
{
    public class SimpleEqualityComparer : IEqualityComparer
    {
        public bool Called;

        bool IEqualityComparer.Equals(object x, object y)
        {
            Called = true;
#if SILVERLIGHT
            return Comparer<object>.Default.Compare(x, y) == 0;
#else
                return Comparer.Default.Compare(x, y) == 0;
#endif
        }

        int IEqualityComparer.GetHashCode(object x)
        {
            return x.GetHashCode();
        }
    }

    public class SimpleEqualityComparer<T> : IEqualityComparer<T>
    {
        public bool Called;

        bool IEqualityComparer<T>.Equals(T x, T y)
        {
            Called = true;
            return Comparer<T>.Default.Compare(x, y) == 0;
        }

        int IEqualityComparer<T>.GetHashCode(T x)
        {
            return x.GetHashCode();
        }
    }
}
#endif
