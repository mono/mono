/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if CLR2
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Ast;
#else
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
#endif

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Dynamic.Utils {
    internal static class CollectionExtensions {
        /// <summary>
        /// Wraps the provided enumerable into a ReadOnlyCollection{T}
        /// 
        /// Copies all of the data into a new array, so the data can't be
        /// changed after creation. The exception is if the enumerable is
        /// already a ReadOnlyCollection{T}, in which case we just return it.
        /// </summary>
#if !CLR2
        [Pure]
#endif
        internal static ReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> enumerable) {
            if (enumerable == null) {
                return EmptyReadOnlyCollection<T>.Instance;
            }

#if SILVERLIGHT
            if (Expression.SilverlightQuirks) {
                // Allow any ReadOnlyCollection to be stored directly
                // (even though this is not safe)
                var r = enumerable as ReadOnlyCollection<T>;
                if (r != null) {
                    return r;
                }
            }
#endif

            var troc = enumerable as TrueReadOnlyCollection<T>;
            if (troc != null) {
                return troc;
            }

            var builder = enumerable as ReadOnlyCollectionBuilder<T>;
            if (builder != null) {
                return builder.ToReadOnlyCollection();
            }

            var collection = enumerable as ICollection<T>;
            if (collection != null) {
                int count = collection.Count;
                if (count == 0) {
                    return EmptyReadOnlyCollection<T>.Instance;
                }

                T[] clone = new T[count];
                collection.CopyTo(clone, 0);
                return new TrueReadOnlyCollection<T>(clone);
            }

            // ToArray trims the excess space and speeds up access
            return new TrueReadOnlyCollection<T>(new List<T>(enumerable).ToArray());
        }

        // We could probably improve the hashing here
        internal static int ListHashCode<T>(this IEnumerable<T> list) {
            var cmp = EqualityComparer<T>.Default;
            int h = 6551;
            foreach (T t in list) {
                h ^= (h << 5) ^ cmp.GetHashCode(t);
            }
            return h;
        }

#if !CLR2
        [Pure]
#endif
        internal static bool ListEquals<T>(this ICollection<T> first, ICollection<T> second) {
            if (first.Count != second.Count) {
                return false;
            }
            var cmp = EqualityComparer<T>.Default;
            var f = first.GetEnumerator();
            var s = second.GetEnumerator();
            while (f.MoveNext()) {
                s.MoveNext();

                if (!cmp.Equals(f.Current, s.Current)) {
                    return false;
                }
            }
            return true;
        }

        internal static IEnumerable<U> Select<T, U>(this IEnumerable<T> enumerable, Func<T, U> select) {
            foreach (T t in enumerable) {
                yield return select(t);
            }
        }

        // Name needs to be different so it doesn't conflict with Enumerable.Select
        internal static U[] Map<T, U>(this ICollection<T> collection, Func<T, U> select) {
            int count = collection.Count;
            U[] result = new U[count];
            count = 0;
            foreach (T t in collection) {
                result[count++] = select(t);
            }
            return result;
        }

        internal static IEnumerable<T> Where<T>(this IEnumerable<T> enumerable, Func<T, bool> where) {
            foreach (T t in enumerable) {
                if (where(t)) {
                    yield return t;
                }
            }
        }

        internal static bool Any<T>(this IEnumerable<T> source, Func<T, bool> predicate) {
            foreach (T element in source) {
                if (predicate(element)) {
                    return true;
                }
            }
            return false;
        }

        internal static bool All<T>(this IEnumerable<T> source, Func<T, bool> predicate) {
            foreach (T element in source) {
                if (!predicate(element)) {
                    return false;
                }
            }
            return true;
        }

        internal static T[] RemoveFirst<T>(this T[] array) {
            T[] result = new T[array.Length - 1];
            Array.Copy(array, 1, result, 0, result.Length);
            return result;
        }

        internal static T[] RemoveLast<T>(this T[] array) {
            T[] result = new T[array.Length - 1];
            Array.Copy(array, 0, result, 0, result.Length);
            return result;
        }

        internal static T[] AddFirst<T>(this IList<T> list, T item) {
            T[] res = new T[list.Count + 1];
            res[0] = item;
            list.CopyTo(res, 1);
            return res;
        }

        internal static T[] AddLast<T>(this IList<T> list, T item) {
            T[] res = new T[list.Count + 1];
            list.CopyTo(res, 0);
            res[list.Count] = item;
            return res;
        }

        internal static T First<T>(this IEnumerable<T> source) {
            var list = source as IList<T>;
            if (list != null) {
                return list[0];
            }
            using (var e = source.GetEnumerator()) {
                if (e.MoveNext()) return e.Current;
            }
            throw new InvalidOperationException();
        }

        internal static T Last<T>(this IList<T> list) {
            return list[list.Count - 1];
        }

        internal static T[] Copy<T>(this T[] array) {
            T[] copy = new T[array.Length];
            Array.Copy(array, copy, array.Length);
            return copy;
        }
    }


    internal static class EmptyReadOnlyCollection<T> {
        internal static ReadOnlyCollection<T> Instance = new TrueReadOnlyCollection<T>(new T[0]);
    }
}
