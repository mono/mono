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
using System.Runtime.CompilerServices;
#if !CODEPLEX_40
using Microsoft.Runtime.CompilerServices;
#endif


#if CODEPLEX_40
namespace System.Dynamic.Utils {
#else
namespace Microsoft.Scripting.Utils {
#endif
    internal sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> {
        internal static readonly ReferenceEqualityComparer<T> Instance = new ReferenceEqualityComparer<T>();

        private ReferenceEqualityComparer() { }

        public bool Equals(T x, T y) {
            return object.ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj) {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
