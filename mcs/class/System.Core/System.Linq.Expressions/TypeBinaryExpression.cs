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
//

namespace System.Linq.Expressions
{
    public sealed class TypeBinaryExpression : Expression
    {
        #region .ctor
        internal TypeBinaryExpression(ExpressionType nt, Expression expression, Type typeop, Type resultType)
            : base(nt, resultType)
        {
            this.expression = expression;
            this.typeop = typeop;
        }
        #endregion

        #region Fields
        private Expression expression;
        private Type typeop;
        #endregion

        #region Properties
        public Expression Expression
        {
            get { return expression; }
        }

        public Type TypeOperand
        {
            get { return typeop; }
        }
        #endregion

        #region Internal Methods
        internal override void BuildString(System.Text.StringBuilder builder)
        {
            //TODO:
        }
        #endregion
    }
}