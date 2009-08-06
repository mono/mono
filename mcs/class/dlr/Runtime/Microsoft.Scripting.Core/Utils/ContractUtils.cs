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
using System.Diagnostics;
#if CODEPLEX_40
using System.Linq.Expressions;
#else
using Microsoft.Linq.Expressions;
#endif

#if SILVERLIGHT
using System.Core;
#endif

#if CODEPLEX_40
namespace System.Dynamic.Utils {
#else
namespace Microsoft.Scripting.Utils {
#endif

    // Will be replaced with CLRv4 managed contracts
    internal static class ContractUtils {

        internal static Exception Unreachable {
            get {
                Debug.Assert(false, "Unreachable");
                return new InvalidOperationException("Code supposed to be unreachable");
            }
        }

        internal static void Requires(bool precondition) {
            if (!precondition) {
                throw new ArgumentException(Strings.MethodPreconditionViolated);
            }
        }

        internal static void Requires(bool precondition, string paramName) {
            Debug.Assert(!string.IsNullOrEmpty(paramName));

            if (!precondition) {
                throw new ArgumentException(Strings.InvalidArgumentValue, paramName);
            }
        }

        internal static void Requires(bool precondition, string paramName, string message) {
            Debug.Assert(!string.IsNullOrEmpty(paramName));

            if (!precondition) {
                throw new ArgumentException(message, paramName);
            }
        }

        internal static void RequiresNotNull(object value, string paramName) {
            Debug.Assert(!string.IsNullOrEmpty(paramName));

            if (value == null) {
                throw new ArgumentNullException(paramName);
            }
        }

        internal static void RequiresNotEmpty<T>(ICollection<T> collection, string paramName) {
            RequiresNotNull(collection, paramName);
            if (collection.Count == 0) {
                throw new ArgumentException(Strings.NonEmptyCollectionRequired, paramName);
            }
        }

        /// <summary>
        /// Requires the range [offset, offset + count] to be a subset of [0, array.Count].
        /// </summary>
        /// <exception cref="ArgumentNullException">Array is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Offset or count are out of range.</exception>
        internal static void RequiresArrayRange<T>(IList<T> array, int offset, int count, string offsetName, string countName) {
            Debug.Assert(!string.IsNullOrEmpty(offsetName));
            Debug.Assert(!string.IsNullOrEmpty(countName));
            Debug.Assert(array != null);

            if (count < 0) throw new ArgumentOutOfRangeException(countName);
            if (offset < 0 || array.Count - offset < count) throw new ArgumentOutOfRangeException(offsetName);
        }

        /// <summary>
        /// Requires the array and all its items to be non-null.
        /// </summary>
        internal static void RequiresNotNullItems<T>(IList<T> array, string arrayName) {
            Debug.Assert(arrayName != null);
            RequiresNotNull(array, arrayName);

            for (int i = 0; i < array.Count; i++) {
                if (array[i] == null) {
                    throw new ArgumentNullException(string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0}[{1}]", arrayName, i));
                }
            }
        }
    }
}
