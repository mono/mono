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

using System.Dynamic.Utils;

namespace System.Dynamic {

    /// <summary>
    /// Represents the dynamic delete member operation at the call site, providing the binding semantic and the details about the operation.
    /// </summary>
    public abstract class DeleteMemberBinder : DynamicMetaObjectBinder {
        private readonly string _name;
        private readonly bool _ignoreCase;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteIndexBinder" />.
        /// </summary>
        /// <param name="name">The name of the member to delete.</param>
        /// <param name="ignoreCase">true if the name should be matched ignoring case; false otherwise.</param>
        protected DeleteMemberBinder(string name, bool ignoreCase) {
            ContractUtils.RequiresNotNull(name, "name");

            _name = name;
            _ignoreCase = ignoreCase;
        }

        /// <summary>
        /// Gets the name of the member to delete.
        /// </summary>
        public string Name {
            get {
                return _name;
            }
        }

        /// <summary>
        /// Gets the value indicating if the string comparison should ignore the case of the member name.
        /// </summary>
        public bool IgnoreCase {
            get {
                return _ignoreCase;
            }
        }

        /// <summary>
        /// The result type of the operation.
        /// </summary>
        public override sealed Type ReturnType {
            get { return typeof(void); }
        }

        /// <summary>
        /// Performs the binding of the dynamic delete member operation if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic delete member operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public DynamicMetaObject FallbackDeleteMember(DynamicMetaObject target) {
            return FallbackDeleteMember(target, null);
        }

        /// <summary>
        /// When overridden in the derived class, performs the binding of the dynamic delete member operation if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic delete member operation.</param>
        /// <param name="errorSuggestion">The binding result to use if binding fails, or null.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public abstract DynamicMetaObject FallbackDeleteMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion);

        /// <summary>
        /// Performs the binding of the dynamic delete member operation.
        /// </summary>
        /// <param name="target">The target of the dynamic delete member operation.</param>
        /// <param name="args">An array of arguments of the dynamic delete member operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.Requires(args == null || args.Length == 0);

            return target.BindDeleteMember(this);
        }


        // this is a standard DynamicMetaObjectBinder
        internal override sealed bool IsStandardBinder {
            get {
                return true;
            }
        }
    }
}
