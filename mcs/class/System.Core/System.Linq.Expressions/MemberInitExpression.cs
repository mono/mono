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

using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Linq.Expressions
{
    public sealed class MemberInitExpression : Expression
    {
        #region .ctor
        internal MemberInitExpression(NewExpression newExpression, ReadOnlyCollection<MemberBinding> bindings)
            : base(ExpressionType.MemberInit, newExpression.Type)
        {
            this.newExpression = newExpression;
            this.bindings = bindings;
        }
        #endregion

        #region Fields
        private ReadOnlyCollection<MemberBinding> bindings;
        private NewExpression newExpression;
        #endregion

        #region Properties
        public ReadOnlyCollection<MemberBinding> Bindings
        {
            get { return bindings; }
        }

        public NewExpression NewExpression
        {
            get { return newExpression; }
        }
        #endregion
    }
}