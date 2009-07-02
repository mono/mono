
/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System; using Microsoft;


using System.Collections.Generic;
#if CODEPLEX_40
using System.Linq.Expressions;
#else
using Microsoft.Linq.Expressions;
#endif

#if CODEPLEX_40
namespace System.Dynamic.Utils {
#else
namespace Microsoft.Scripting.Utils {
#endif
    // Miscellaneous helpers that don't belong anywhere else
    internal static class Helpers {

        internal static T CommonNode<T>(T first, T second, Func<T, T> parent) where T : class {
            var cmp = EqualityComparer<T>.Default;
            if (cmp.Equals(first, second)) {
                return first;
            }
            var set = new Set<T>(cmp);
            for (T t = first; t != null; t = parent(t)) {
                set.Add(t);
            }
            for (T t = second; t != null; t = parent(t)) {
                if (set.Contains(t)) {
                    return t;
                }
            }
            return null;
        }

        internal static void IncrementCount<T>(T key, Dictionary<T, int> dict) {
            int count;
            dict.TryGetValue(key, out count);
            dict[key] = count + 1;
        }
    }
}
