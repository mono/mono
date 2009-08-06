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

#if CODEPLEX_40
using System.Linq.Expressions;
#else
using Microsoft.Linq.Expressions;
#endif
using System.Runtime.CompilerServices;
#if !CODEPLEX_40
using Microsoft.Runtime.CompilerServices;
#endif

using System.Security;
using System.Security.Permissions;

#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif
    internal sealed class BoundDispEvent : DynamicObject {
        private object _rcw;
        private Guid _sourceIid;
        private int _dispid;

        internal BoundDispEvent(object rcw, Guid sourceIid, int dispid) {
            _rcw = rcw;
            _sourceIid = sourceIid;
            _dispid = dispid;
        }

        /// <summary>
        /// Provides the implementation of performing AddAssign and SubtractAssign binary operations.
        /// </summary>
        /// <param name="binder">The binder provided by the call site.</param>
        /// <param name="handler">The handler for the operation.</param>
        /// <param name="result">The result of the operation.</param>
        /// <returns>true if the operation is complete, false if the call site should determine behavior.</returns>
        public override bool TryBinaryOperation(BinaryOperationBinder binder, object handler, out object result) {
            if (binder.Operation == ExpressionType.AddAssign) {
                result = InPlaceAdd(handler);
                return true;
            }

            if (binder.Operation == ExpressionType.SubtractAssign) {
                result = InPlaceSubtract(handler);
                return true;
            }

            result = null;
            return false;
        }

        private static void VerifyHandler(object handler) {
            if (handler is Delegate && handler.GetType() != typeof(Delegate)) {
                return; // delegate
            }

            if (handler is IDynamicMetaObjectProvider) {
                return; // IDMOP
            }

            throw Error.UnsupportedHandlerType();
        }

        /// <summary>
        /// Adds a handler to an event.
        /// </summary>
        /// <param name="handler">The handler to be added.</param>
        /// <returns>The original event with handler added.</returns>
#if CLR2
        [SecurityCritical, SecurityTreatAsSafe]
#else
        [SecuritySafeCritical]
#endif
        private object InPlaceAdd(object handler) {
            ContractUtils.RequiresNotNull(handler, "handler");
            VerifyHandler(handler);

            //
            // Demand Full Trust to proceed with the operation.
            //

            new PermissionSet(PermissionState.Unrestricted).Demand();

            ComEventSink comEventSink = ComEventSink.FromRuntimeCallableWrapper(_rcw, _sourceIid, true);
            comEventSink.AddHandler(_dispid, handler);
            return this;
        }

        /// <summary>
        /// Removes handler from the event.
        /// </summary>
        /// <param name="handler">The handler to be removed.</param>
        /// <returns>The original event with handler removed.</returns>
#if CLR2
        [SecurityCritical, SecurityTreatAsSafe]
#else
        [SecuritySafeCritical]
#endif
        private object InPlaceSubtract(object handler) {
            ContractUtils.RequiresNotNull(handler, "handler");
            VerifyHandler(handler);

            //
            // Demand Full Trust to proceed with the operation.
            //

            new PermissionSet(PermissionState.Unrestricted).Demand();

            ComEventSink comEventSink = ComEventSink.FromRuntimeCallableWrapper(_rcw, _sourceIid, false);
            if (comEventSink != null) {
                comEventSink.RemoveHandler(_dispid, handler);
            }

            return this;
        }
    }
}

#endif
