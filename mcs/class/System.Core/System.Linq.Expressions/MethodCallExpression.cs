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
//        Federico Di Gregorio <fog@initd.org>
//

using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

namespace System.Linq.Expressions
{
    public sealed class MethodCallExpression : Expression
    {
        #region .ctor
        internal MethodCallExpression(ExpressionType type, MethodInfo method, Expression obj, ReadOnlyCollection<Expression> arguments)
            : base(type, method.ReturnType)
        {
            this.obj = obj;
            this.method = method;
            this.arguments = arguments;
        }
        #endregion

        #region Fields
        private ReadOnlyCollection<Expression> arguments;
        private MethodInfo method;
        private Expression obj;
        #endregion

        #region Properties
        public ReadOnlyCollection<Expression> Arguments {
            get { return arguments; }
        }

        public MethodInfo Method {
            get { return method; }
        }

        public Expression Object {
            get { return obj; }
        }
        #endregion

        #region Internal Methods
        internal override void BuildString(StringBuilder builder)
        {
            obj.BuildString(builder);
            builder.Append (".").Append (method.Name).Append ("(");
            for (int i=0 ; i < arguments.Count ; i++) {
                arguments [i].BuildString (builder);
                if (i < arguments.Count-1)
                    builder.Append (", ");
            }
            builder.Append(")");
        }
        #endregion
    }
}
