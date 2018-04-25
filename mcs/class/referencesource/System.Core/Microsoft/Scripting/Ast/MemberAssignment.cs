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
using System.Dynamic.Utils;
using System.Reflection;

#if SILVERLIGHT
using System.Core;
#endif

#if CLR2
namespace Microsoft.Scripting.Ast {
#else
namespace System.Linq.Expressions {
#endif
    /// <summary>
    /// Represents assignment to a member of an object.
    /// </summary>
    public sealed class MemberAssignment : MemberBinding {
        Expression _expression;
        internal MemberAssignment(MemberInfo member, Expression expression)
#pragma warning disable 618
            : base(MemberBindingType.Assignment, member) {
#pragma warning restore 618
            _expression = expression;
        }
        /// <summary>
        /// Gets the <see cref="Expression"/> which represents the object whose member is being assigned to.
        /// </summary>
        public Expression Expression {
            get { return _expression; }
        }

        /// <summary>
        /// Creates a new expression that is like this one, but using the
        /// supplied children. If all of the children are the same, it will
        /// return this expression.
        /// </summary>
        /// <param name="expression">The <see cref="Expression" /> property of the result.</param>
        /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
        public MemberAssignment Update(Expression expression) {
            if (expression == Expression) {
                return this;
            }
            return Expression.Bind(Member, expression);
        }
    }


    public partial class Expression {
        /// <summary>
        /// Creates a <see cref="MemberAssignment"/> binding the specified value to the given member.
        /// </summary>
        /// <param name="member">The <see cref="MemberInfo"/> for the member which is being assigned to.</param>
        /// <param name="expression">The value to be assigned to <paramref name="member"/>.</param>
        /// <returns>The created <see cref="MemberAssignment"/>.</returns>
        public static MemberAssignment Bind(MemberInfo member, Expression expression) {
            ContractUtils.RequiresNotNull(member, "member");
            RequiresCanRead(expression, "expression");
            Type memberType;
            ValidateSettableFieldOrPropertyMember(member, out memberType);
            if (!memberType.IsAssignableFrom(expression.Type)) {
                throw Error.ArgumentTypesMustMatch();
            }
            return new MemberAssignment(member, expression);
        }

        /// <summary>
        /// Creates a <see cref="MemberAssignment"/> binding the specified value to the given property.
        /// </summary>
        /// <param name="propertyAccessor">The <see cref="PropertyInfo"/> for the property which is being assigned to.</param>
        /// <param name="expression">The value to be assigned to <paramref name="propertyAccessor"/>.</param>
        /// <returns>The created <see cref="MemberAssignment"/>.</returns>
        public static MemberAssignment Bind(MethodInfo propertyAccessor, Expression expression) {
            ContractUtils.RequiresNotNull(propertyAccessor, "propertyAccessor");
            ContractUtils.RequiresNotNull(expression, "expression");
            ValidateMethodInfo(propertyAccessor);
            return Bind(GetProperty(propertyAccessor), expression);
        }
        

        private static void ValidateSettableFieldOrPropertyMember(MemberInfo member, out Type memberType) {
            FieldInfo fi = member as FieldInfo;
            if (fi == null) {
                PropertyInfo pi = member as PropertyInfo;
                if (pi == null) {
                    throw Error.ArgumentMustBeFieldInfoOrPropertInfo();
                }
                if (!pi.CanWrite) {
                    throw Error.PropertyDoesNotHaveSetter(pi);
                }
                memberType = pi.PropertyType;
            } else {
                memberType = fi.FieldType;
            }
        }
    }
}
