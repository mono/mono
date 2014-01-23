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

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using System.Dynamic;

namespace Microsoft.Scripting.Utils {
    public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly ReferenceEqualityComparer<T> Instance = new ReferenceEqualityComparer<T>();

        private ReferenceEqualityComparer() { }

        public bool Equals(T x, T y) {
            return object.ReferenceEquals(x, y);
        }

#if WIN8
        private static Expression NullConst = Expression.Constant(null);
        private static int H = 536870912 ^ NullConst.GetHashCode();
#endif

        public int GetHashCode(T obj) {
#if WP75 // CF RH.GetHashCode throws NullReferenceException if the argument is null
            return obj != null ? RuntimeHelpers.GetHashCode(obj) : 0;
#elif WIN8
            // TODO: HACK!
            return BindingRestrictions.GetInstanceRestriction(NullConst, obj).GetHashCode() ^ H;
#else
            return RuntimeHelpers.GetHashCode(obj);
#endif
        }
    }
}
