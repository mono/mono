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

using System.Reflection;

namespace System.Linq.Expressions
{
    public sealed class UnaryExpression : Expression
    {
        #region .ctor
        internal UnaryExpression(ExpressionType nt, Expression operand, MethodInfo method, Type type)
            : base(nt, type)
        {
            this.operand = operand;
            this.method = method;
        }

        internal UnaryExpression(ExpressionType nt, Expression operand, Type type)
            : this(nt, operand, null, type)
        {
        }
        #endregion

        #region Fields
        private MethodInfo method;
        private Expression operand;
        #endregion

        #region Properties
        //TODO:
        public bool IsLifted
        {
            get { return false; }
        }

        //TODO:
        public bool IsLiftedToNull
        {
            get { return false; }
        }

        public MethodInfo Method
        {
            get { return method; }
        }

        public Expression Operand
        {
            get { return operand; }
        }
        #endregion
    }
}