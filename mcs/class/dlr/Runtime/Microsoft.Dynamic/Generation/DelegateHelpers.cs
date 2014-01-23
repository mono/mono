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

using System;
using System.Collections.Generic;
using System.Reflection;
#if FEATURE_REFEMIT
using System.Reflection.Emit;
#endif
using System.Threading;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Generation {
    internal static partial class DelegateHelpers {

        private static Dictionary<ICollection<Type>, Type> _DelegateTypes;

        private static Type MakeCustomDelegate(Type[] types) {
            if (_DelegateTypes == null) {
                Interlocked.CompareExchange(
                    ref _DelegateTypes,
                    new Dictionary<ICollection<Type>, Type>(ListEqualityComparer<Type>.Instance),
                    null
                );
            }

            bool found;
            Type type;

            //
            // LOCK to retrieve the delegate type, if any
            //

            lock (_DelegateTypes) {
                found = _DelegateTypes.TryGetValue(types, out type);
            }

            if (!found && type != null) {
                return type;
            }

            //
            // Create new delegate type
            //

            type = MakeNewCustomDelegate(types);

            //
            // LOCK to insert new delegate into the cache. If we already have one (racing threads), use the one from the cache
            //

            lock (_DelegateTypes) {
                Type conflict;
                if (_DelegateTypes.TryGetValue(types, out conflict) && conflict != null) {
                    type = conflict;
                } else {
                    _DelegateTypes[types] = type;
                }
            }

            return type;
        }

        private static Type MakeNewCustomDelegate(Type[] types) {
#if FEATURE_REFEMIT
            Type returnType = types[types.Length - 1];
            Type[] parameters = types.RemoveLast();

            return Snippets.Shared.DefineDelegate("Delegate" + types.Length, returnType, parameters);
#else
            throw new NotSupportedException("Signature not supported on this platform");
#endif
        }
    }
}
