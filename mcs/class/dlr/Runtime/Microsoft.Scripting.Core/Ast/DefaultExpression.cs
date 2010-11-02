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
    /// Represents the default value of a type or an empty expression.
    /// </summary>
#if !SILVERLIGHT
    [DebuggerTypeProxy(typeof(Expression.DefaultExpressionProxy))]
#endif
    public sealed class DefaultExpression : Expression {
        private readonly Type _type;

        internal DefaultExpression(Type type) {
            _type = type;
        }

        /// <summary>
        /// Gets the static type of the expression that this <see cref="Expression" /> represents.
        /// </summary>
        /// <returns>The <see cref="Type"/> that represents the static type of the expression.</returns>
        public sealed override Type Type {
            get { return _type; }
        }

        /// <summary>
        /// Returns the node type of this Expression. Extension nodes should return
        /// ExpressionType.Extension when overriding this method.
        /// </summary>
        /// <returns>The <see cref="ExpressionType"/> of the expression.</returns>
        public sealed override ExpressionType NodeType {
            get { return ExpressionType.Default; }
        }

        /// <summary>
        /// Dispatches to the specific visit method for this node type.
        /// </summary>
        protected internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitDefault(this);
        }
    }

    public partial class Expression {
        /// <summary>
        /// Creates an empty expression that has <see cref="System.Void"/> type.
        /// </summary>
        /// <returns>
        /// A <see cref="DefaultExpression"/> that has the <see cref="P:Expression.NodeType"/> property equal to 
        /// <see cref="F:ExpressionType.Default"/> and the <see cref="P:Expression.Type"/> property set to <see cref="System.Void"/>.
        /// </returns>
        public static DefaultExpression Empty() {
            return new DefaultExpression(typeof(void));
        }

        /// <summary>
        /// Creates a <see cref="DefaultExpression"/> that has the <see cref="P:Expression.Type"/> property set to the specified type.
        /// </summary>
        /// <param name="type">A <see cref="System.Type"/> to set the <see cref="P:Expression.Type"/> property equal to.</param>
        /// <returns>
        /// A <see cref="DefaultExpression"/> that has the <see cref="P:Expression.NodeType"/> property equal to 
        /// <see cref="F:ExpressionType.Default"/> and the <see cref="P:Expression.Type"/> property set to the specified type.
        /// </returns>
        public static DefaultExpression Default(Type type) {
            if (type == typeof(void)) {
                return Empty();
            }
            return new DefaultExpression(type);
        }
    }
}
