using System.Collections;

namespace NUnit.TestUtilities
{
    public class SimpleObjectComparer : IComparer
    {
        public bool Called;

        public int Compare(object x, object y)
        {
            Called = true;
#if SILVERLIGHT
            return System.Collections.Generic.Comparer<object>.Default.Compare(x, y);
#else
            return System.Collections.Comparer.Default.Compare(x, y);
#endif
        }
    }
}
