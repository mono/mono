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
using System.Text;

namespace System.Linq.Expressions
{
    public sealed class BinaryExpression : Expression
    {
        #region .ctor
        internal BinaryExpression(ExpressionType nt, Expression left, Expression right, MethodInfo method, Type type)
            : base(nt, type)
        {
            this.left = left;
            this.right = right;
            this.method = method;
        }

        internal BinaryExpression(ExpressionType nt, Expression left, Expression right, Type type)
            : this(nt, left, right, null, type)
        {
        }
        #endregion

        #region Fields
        private Expression left;
        private Expression right;
        private MethodInfo method;
        #endregion

        #region Properties
        public Expression Left
        {
            get { return left; }
        }

        public Expression Right
        {
            get { return right; }
        }

        public MethodInfo Method
        {
            get { return method; }
        }

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
        #endregion

        #region Internal Methods
        internal override void BuildString(StringBuilder builder)
        {
            //TODO:
        }
        #endregion
    }
}