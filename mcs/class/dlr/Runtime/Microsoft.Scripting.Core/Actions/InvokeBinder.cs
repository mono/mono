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


#if CODEPLEX_40
using System.Dynamic.Utils;
#else
using Microsoft.Scripting.Utils;
#endif

#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif

    /// <summary>
    /// Represents the invoke dynamic operation at the call site, providing the binding semantic and the details about the operation.
    /// </summary>
    public abstract class InvokeBinder : DynamicMetaObjectBinder {
        private readonly CallInfo _callInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokeBinder" />.
        /// </summary>
        /// <param name="callInfo">The signature of the arguments at the call site.</param>
        protected InvokeBinder(CallInfo callInfo) {
            ContractUtils.RequiresNotNull(callInfo, "callInfo");
            _callInfo = callInfo;
        }

        /// <summary>
        /// The result type of the operation.
        /// </summary>
        public override sealed Type ReturnType {
            get { return typeof(object); }
        }

        /// <summary>
        /// Gets the signature of the arguments at the call site.
        /// </summary>
        public CallInfo CallInfo {
            get { return _callInfo; }
        }

        /// <summary>
        /// Performs the binding of the dynamic invoke operation if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic invoke operation.</param>
        /// <param name="args">The arguments of the dynamic invoke operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args) {
            return FallbackInvoke(target, args, null);
        }

        /// <summary>
        /// Performs the binding of the dynamic invoke operation if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic invoke operation.</param>
        /// <param name="args">The arguments of the dynamic invoke operation.</param>
        /// <param name="errorSuggestion">The binding result to use if binding fails, or null.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public abstract DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion);

        /// <summary>
        /// Performs the binding of the dynamic invoke operation.
        /// </summary>
        /// <param name="target">The target of the dynamic invoke operation.</param>
        /// <param name="args">An array of arguments of the dynamic invoke operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNullItems(args, "args");

            return target.BindInvoke(this, args);
        }

        // this is a standard DynamicMetaObjectBinder
        internal override sealed bool IsStandardBinder {
            get {
                return true;
            }
        }
    }
}
