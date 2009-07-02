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


using System.Diagnostics;
#if CODEPLEX_40
using System.Linq.Expressions;
#else
using Microsoft.Linq.Expressions;
#endif
using System.Runtime.CompilerServices;
#if !CODEPLEX_40
using Microsoft.Runtime.CompilerServices;
#endif


#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif

    internal sealed class SplatCallSite {
        // Stored callable Delegate or IDynamicMetaObjectProvider.
        internal readonly object _callable;

        // Can the number of arguments to a given event change each call?
        // If not, we don't need this level of indirection--we could cache a
        // delegate that does the splatting.
        internal CallSite<Func<CallSite, object, object[], object>> _site;

        internal SplatCallSite(object callable) {
            Debug.Assert(callable != null);
            _callable = callable;
        }

        internal object Invoke(object[] args) {
            Debug.Assert(args != null);

            // If it is a delegate, just let DynamicInvoke do the binding.
            var d = _callable as Delegate;
            if (d != null) {
                return d.DynamicInvoke(args);
            }

            // Otherwise, create a CallSite and invoke it.
            if (_site == null) {
                _site = CallSite<Func<CallSite, object, object[], object>>.Create(SplatInvokeBinder.Instance);
            }

            return _site.Target(_site, _callable, args);
        }
    }
}
