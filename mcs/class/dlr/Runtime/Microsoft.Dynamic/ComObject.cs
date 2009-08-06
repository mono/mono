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


#if !SILVERLIGHT // ComObject

using System.Collections.Generic;
using System.Diagnostics;
#if CODEPLEX_40
using System.Linq.Expressions;
#else
using Microsoft.Linq.Expressions;
#endif
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif
    /// <summary>
    /// This is a helper class for runtime-callable-wrappers of COM instances. We create one instance of this type
    /// for every generic RCW instance.
    /// </summary>
    internal class ComObject : IDynamicMetaObjectProvider {
        /// <summary>
        /// The runtime-callable wrapper
        /// </summary>
        private readonly object _rcw;

        internal ComObject(object rcw) {
            Debug.Assert(ComObject.IsComObject(rcw));
            _rcw = rcw;
        }

        internal object RuntimeCallableWrapper {
            get {
                return _rcw;
            }
        }

        private readonly static object _ComObjectInfoKey = new object();

        /// <summary>
        /// This is the factory method to get the ComObject corresponding to an RCW
        /// </summary>
        /// <returns></returns>
#if CLR2
        [PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
#endif
        [SecurityCritical]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        public static ComObject ObjectToComObject(object rcw) {
            Debug.Assert(ComObject.IsComObject(rcw));

            // Marshal.Get/SetComObjectData has a LinkDemand for UnmanagedCode which will turn into
            // a full demand. We could avoid this by making this method SecurityCritical
            object data = Marshal.GetComObjectData(rcw, _ComObjectInfoKey);
            if (data != null) {
                return (ComObject)data;
            }

            lock (_ComObjectInfoKey) {
                data = Marshal.GetComObjectData(rcw, _ComObjectInfoKey);
                if (data != null) {
                    return (ComObject)data;
                }

                ComObject comObjectInfo = CreateComObject(rcw);
                if (!Marshal.SetComObjectData(rcw, _ComObjectInfoKey, comObjectInfo)) {
                    throw Error.SetComObjectDataFailed();
                }

                return comObjectInfo;
            }
        }

        // Expression that unwraps ComObject
        internal static MemberExpression RcwFromComObject(Expression comObject) {
            Debug.Assert(comObject != null && typeof(ComObject).IsAssignableFrom(comObject.Type), "must be ComObject");

            return Expression.Property(
                Helpers.Convert(comObject, typeof(ComObject)),
                typeof(ComObject).GetProperty("RuntimeCallableWrapper", BindingFlags.NonPublic | BindingFlags.Instance)
            );
        }

        // Expression that finds or creates a ComObject that corresponds to given Rcw
        internal static MethodCallExpression RcwToComObject(Expression rcw) {
            return Expression.Call(
                typeof(ComObject).GetMethod("ObjectToComObject"),
                Helpers.Convert(rcw, typeof(object))
            );
        }

        private static ComObject CreateComObject(object rcw) {
            IDispatch dispatchObject = rcw as IDispatch;
            if (dispatchObject != null) {
                // We can do method invocations on IDispatch objects
                return new IDispatchComObject(dispatchObject);
            }

            // There is not much we can do in this case
            return new ComObject(rcw);
        }

        internal virtual IList<string> GetMemberNames(bool dataOnly) {
            return new string[0];
        }

        internal virtual IList<KeyValuePair<string, object>> GetMembers(IEnumerable<string> names) {
            return new KeyValuePair<string, object>[0];
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) {
            return new ComFallbackMetaObject(parameter, BindingRestrictions.Empty, this);
        }

        private static readonly Type ComObjectType = typeof(object).Assembly.GetType("System.__ComObject");

        internal static bool IsComObject(object obj) {
            // we can't use System.Runtime.InteropServices.Marshal.IsComObject(obj) since it doesn't work in partial trust
            return obj != null && ComObjectType.IsAssignableFrom(obj.GetType());
        }

    }
}

#endif
