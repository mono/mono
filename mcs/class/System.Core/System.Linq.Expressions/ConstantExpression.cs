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

using System.Text;

namespace System.Linq.Expressions
{
    public sealed class ConstantExpression : Expression
    {
        #region .ctor
        internal ConstantExpression (object value, Type type) : base (ExpressionType.Constant, type)
        {
            this.value = value;
        }
        #endregion

        #region Fields
        private object value;
        #endregion

        #region Properties
        public object Value {
            get { return value; }
        }
        #endregion

        #region Internal support methods 
        internal override void BuildString(StringBuilder builder)
        {
            string s;
            
            if (value == null)
                builder.Append ("null");
            
            else if (value.GetType() == typeof(string))
                builder.Append ("\"").Append ((string)value).Append ("\"");
                
            else {
                s = value.ToString();
                if (value != null && s == value.GetType().FullName)
                    builder.Append ("value(").Append (s).Append (")");
                else
                    builder.Append(s);
            }
        }
        #endregion
    }
}