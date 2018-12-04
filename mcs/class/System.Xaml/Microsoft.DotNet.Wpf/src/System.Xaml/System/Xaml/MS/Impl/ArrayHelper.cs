// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace System.Xaml
{
    internal static class ArrayHelper
    {
        internal static S[] ConvertArrayType<R, S>(ICollection<R> src, Func<R, S> f)
        {
            if (src == null)
            {
                return null;
            }
            int len = src.Count, n = 0;
            S[] dest = new S[len];
            foreach (R r in src)
            {
                dest[n++] = f(r);
            }
            return dest;
        }

        internal static void ForAll<R>(R[] src, Action<R> f) 
        {
            foreach (R r in src)
                f(r);
        }

        internal static List<T> ToList<T>(IEnumerable<T> src)
        {
            return (src != null)
                ? new List<T>(src)
                : null;
        }
    }
}
