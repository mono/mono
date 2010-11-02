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
using System.Diagnostics;

#if CLR2
namespace Microsoft.Scripting.Ast {
#else
namespace System.Linq.Expressions {
#endif
    /// <summary>
    /// Represents a label, which can be placed in any <see cref="Expression"/> context. If
    /// it is jumped to, it will get the value provided by the corresponding
    /// <see cref="GotoExpression"/>. Otherwise, it gets the value in <see cref="LabelExpression.DefaultValue"/>. If the
    /// <see cref="Type"/> equals System.Void, no value should be provided.
    /// </summary>
#if !SILVERLIGHT
    [DebuggerTypeProxy(typeof(Expression.LabelExpressionProxy))]
#endif
    public sealed class LabelExpression : Expression {
        private readonly Expression _defaultValue;
        private readonly LabelTarget _target;

        internal LabelExpression(LabelTarget label, Expression defaultValue) {
            _target = label;
            _defaultValue = defaultValue;
        }

        /// <summary>
        /// Gets the static type of the expression that this <see cref="Expression" /> represents. (Inherited from <see cref="Expression"/>.)
        /// </summary>
        /// <returns>The <see cref="Type"/> that represents the static type of the expression.</returns>
        public sealed override Type Type {
            get { return _target.Type; }
        }

        /// <summary>
        /// Returns the node type of this <see cref="Expression" />. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="ExpressionType"/> that represents this expression.</returns>
        public sealed override ExpressionType NodeType {
            get { return ExpressionType.Label; }
        }

        /// <summary>
        /// The <see cref="LabelTarget"/> which this label is associated with.
        /// </summary>
        public LabelTarget Target {
            get { return _target; }
        }

        /// <summary>
        /// The value of the <see cref="LabelExpression"/> when the label is reached through
        /// normal control flow (e.g. is not jumped to).
        /// </summary>
        public Expression DefaultValue {
            get { return _defaultValue; }
        }

        /// <summary>
        /// Dispatches to the specific visit method for this node type.
        /// </summary>
        protected internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitLabel(this);
        }

        /// <summary>
        /// Creates a new expression that is like this one, but using the
        /// supplied children. If all of the children are the same, it will
        /// return this expression.
        /// </summary>
        /// <param name="target">The <see cref="Target" /> property of the result.</param>
        /// <param name="defaultValue">The <see cref="DefaultValue" /> property of the result.</param>
        /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
        public LabelExpression Update(LabelTarget target, Expression defaultValue) {
            if (target == Target && defaultValue == DefaultValue) {
                return this;
            }
            return Expression.Label(target, defaultValue);
        }
    }

    public partial class Expression {
        /// <summary>
        /// Creates a <see cref="LabelExpression"/> representing a label with no default value.
        /// </summary>
        /// <param name="target">The <see cref="LabelTarget"/> which this <see cref="LabelExpression"/> will be associated with.</param>
        /// <returns>A <see cref="LabelExpression"/> with no default value.</returns>
        public static LabelExpression Label(LabelTarget target) {
            return Label(target, null);
        }

        /// <summary>
        /// Creates a <see cref="LabelExpression"/> representing a label with the given default value.
        /// </summary>
        /// <param name="target">The <see cref="LabelTarget"/> which this <see cref="LabelExpression"/> will be associated with.</param>
        /// <param name="defaultValue">The value of this <see cref="LabelExpression"/> when the label is reached through normal control flow.</param>
        /// <returns>A <see cref="LabelExpression"/> with the given default value.</returns>
        public static LabelExpression Label(LabelTarget target, Expression defaultValue) {
            ValidateGoto(target, ref defaultValue, "label", "defaultValue");
            return new LabelExpression(target, defaultValue);
        }
    }
}
