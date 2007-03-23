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
using System.Text;

namespace System.Linq.Expressions
{
    public sealed class ListInitExpression : Expression
    {
        #region .ctor
        internal ListInitExpression(NewExpression newExpression, ReadOnlyCollection<Expression> expressions)
            : base(ExpressionType.ListInit, newExpression.Type)
        {
            this.newExpression = newExpression;
            this.expressions = expressions;
        }
        #endregion

        #region Fields
        private ReadOnlyCollection<Expression> expressions;
        private NewExpression newExpression;
        #endregion

        #region Properties
        public ReadOnlyCollection<Expression> Expressions
        {
            get { return expressions; }
        }

        public NewExpression NewExpression
        {
            get { return newExpression; }
        }
        #endregion

        #region Internal Methods
        internal override void BuildString(StringBuilder builder)
        {
            // we need to build the "new" expression before...
            newExpression.BuildString(builder);

            //... and the elements of the list ...
            builder.Append("{");

            int exprc = expressions.Count;
            for (int i = 0; i < exprc; i++)
            {
                expressions[i].BuildString(builder);
                if (i < expressions.Count - 1)
                    builder.Append(", ");
            }

            builder.Append("}");
        }
        #endregion
    }
}