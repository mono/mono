// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//        Antonello Provenzano  <antonello@deveel.com>
//        Federico Di Gregorio  <fog@initd.org>
//

using System.Reflection;
using System.Text;

namespace System.Linq.Expressions
{
    public sealed class MemberExpression : Expression
    {
        #region .ctor
        internal MemberExpression (Expression expression, MemberInfo member, Type type)
            : base(ExpressionType.MemberAccess, type)
        {
            this.expr = expression;
            this.member = member;
        }
        #endregion

        #region Fields
        private Expression expr;
        private MemberInfo member;
        #endregion

        #region Properties
        public Expression Expression {
            get { return expr; }
        }

        public MemberInfo Member {
            get { return member; }
        }
        #endregion

        #region Internal Methods
        internal override void BuildString (StringBuilder builder)
        {
            if (expr != null)
                expr.BuildString (builder);
            else
                builder.Append (member.DeclaringType.Name);
            builder.Append (".").Append (member.Name);
        }
        #endregion
    }
}