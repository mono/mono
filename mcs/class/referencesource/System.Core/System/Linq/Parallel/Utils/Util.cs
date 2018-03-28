// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// Util.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;

namespace System.Linq.Parallel
{
    /// <summary>
    /// Common miscellaneous utility methods used throughout the code-base.
    /// </summary>
    internal static class Util
    {

        //-----------------------------------------------------------------------------------
        // Simple helper that returns a constant depending on the sign of the argument. I.e.
        // if the argument is negative, the result is -1; if it's positive, the result is 1;
        // otherwise, if it's zero, the result is 0.
        //

        internal static int Sign(int x)
        {
            return x < 0 ? -1 : x == 0 ? 0 : 1;
        }

        //-----------------------------------------------------------------------------------
        // This is a temporary workaround for a VSWhidbey bug 601998 in the X64 JIT compiler.
        // Unlike the X86 JIT, null checks on value types aren't optimized away in Whidbey.
        // That means using the GenericComparer<K> infrastructure results in boxing value
        // types. This kills performance all over the place. This bug has been fixed in
        // Orcas (2.0 SP1), so once we're on the SP1 runtime, this can be removed.
        //

        internal static Comparer<TKey> GetDefaultComparer<TKey>()
        {
            if (typeof(TKey) == typeof(int))
            {
                return (Comparer<TKey>)(object)s_fastIntComparer;
            }
            else if (typeof(TKey) == typeof(long))
            {
                return (Comparer<TKey>)(object)s_fastLongComparer;
            }
            else if (typeof(TKey) == typeof(float))
            {
                return (Comparer<TKey>)(object)s_fastFloatComparer;
            }
            else if (typeof(TKey) == typeof(double))
            {
                return (Comparer<TKey>)(object)s_fastDoubleComparer;
            }
            else if (typeof(TKey) == typeof(DateTime))
            {
                return (Comparer<TKey>)(object)s_fastDateTimeComparer;
            }

            return Comparer<TKey>.Default;
        }

        private static FastIntComparer s_fastIntComparer = new FastIntComparer();

        class FastIntComparer : Comparer<int>
        {
            public override int Compare(int x, int y)
            {
                return x.CompareTo(y);
            }
        }

        private static FastLongComparer s_fastLongComparer = new FastLongComparer();

        class FastLongComparer : Comparer<long>
        {
            public override int Compare(long x, long y)
            {
                return x.CompareTo(y);
            }
        }

        private static FastFloatComparer s_fastFloatComparer = new FastFloatComparer();

        class FastFloatComparer : Comparer<float>
        {
            public override int Compare(float x, float y)
            {
                return x.CompareTo(y);
            }
        }

        private static FastDoubleComparer s_fastDoubleComparer = new FastDoubleComparer();

        class FastDoubleComparer : Comparer<double>
        {
            public override int Compare(double x, double y)
            {
                return x.CompareTo(y);
            }
        }

        private static FastDateTimeComparer s_fastDateTimeComparer = new FastDateTimeComparer();

        class FastDateTimeComparer : Comparer<DateTime>
        {
            public override int Compare(DateTime x, DateTime y)
            {
                return x.CompareTo(y);
            }
        }

    }
}
