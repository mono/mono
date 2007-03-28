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
//        Marek Safar (marek.safar@seznam.cz)
//        Antonello Provenzano  <antonello@deveel.com>
//

using System;
using System.Text;

namespace System.Linq.Expressions
{
    public sealed class ParameterExpression : Expression
    {
        #region .ctor
        internal ParameterExpression(Type type, string name)
            : base(ExpressionType.Parameter, type) {
            this.name = name;
        }
        #endregion

        #region Fields
        private string name;
        #endregion

        #region Properties
        public string Name
        {
            get { return name; }
        }
        #endregion

        #region Internal Methods
        internal override void BuildString(StringBuilder builder)
        {
            string param = name;
            if (param == null)
                param = "<param>";
            builder.Append(param);
        }
        #endregion
    }
}
