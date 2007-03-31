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
    [Obsolete]
    public sealed class LiftExpression : Expression
    {
        #region .ctor
        internal LiftExpression(ExpressionType eType, Type resultType, Expression expression, ReadOnlyCollection<ParameterExpression> parameters, ReadOnlyCollection<Expression> arguments)
            : base(eType, resultType)
        {
            this.expression = expression;
            this.parameters = parameters;
            this.arguments = arguments;
        }
        #endregion

        #region Fields
        private ReadOnlyCollection<Expression> arguments;
        private Expression expression;
        private ReadOnlyCollection<ParameterExpression> parameters;
        #endregion

        #region Properties
        public ReadOnlyCollection<Expression> Arguments
        {
            get { return arguments; }
        }

        public Expression Expression
        {
            get { return expression; }
        }

        public ReadOnlyCollection<ParameterExpression> Parameters
        {
            get { return parameters; }
        }
        #endregion

        #region Internal Methods
        internal override void BuildString(StringBuilder builder)
        {
            builder.Append(NodeType);
            builder.Append("(");

            int parCount = parameters.Count;
            for (int i = 0; i < parCount; i++)
            {
                parameters[i].BuildString(builder);
                builder.Append("=");
                arguments[i].BuildString(builder);

                if (i < parCount - 1)
                    builder.Append(", ");
            }

            builder.Append(", ");
            expression.BuildString(builder);

            builder.Append(")");
        }
        #endregion
    }
}