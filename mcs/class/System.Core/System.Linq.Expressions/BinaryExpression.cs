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
    public sealed class BinaryExpression : Expression
    {
        #region .ctor
        internal BinaryExpression (ExpressionType nt, Expression left, Expression right, MethodInfo method, Type type)
            : base(nt, type)
        {
            this.left = left;
            this.right = right;
            this.method = method;
        }

        internal BinaryExpression (ExpressionType nt, Expression left, Expression right, Type type)
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
        public Expression Left {
            get { return left; }
        }

        public Expression Right {
            get { return right; }
        }

        public MethodInfo Method {
            get { return method; }
        }

        [MonoTODO]
        public bool IsLifted {
            get { return false; }
        }

        [MonoTODO]
        public bool IsLiftedToNull {
            get { return false; }
        }
        #endregion

        #region Internal Methods
        internal override void BuildString(StringBuilder builder)
        {
            switch (NodeType)
            {
            case ExpressionType.Add:
                builder.AppendFormat ("({0} + {1})", left, right);
                break;
                
            case ExpressionType.AddChecked:
                builder.AppendFormat ("({0} + {1})", left, right);
                break;

            // See below for ExpressionType.And.
            
            case ExpressionType.AndAlso:
                builder.AppendFormat ("({0} && {1})", left, right);
                break;
            
            case ExpressionType.ArrayIndex:
                builder.AppendFormat ("{0}[{1}]", left, right);
                break;

            case ExpressionType.Coalesce:
                builder.AppendFormat ("({0} ?? {1})", left, right);
                break;

            case ExpressionType.Divide:
                builder.AppendFormat ("({0} / {1})", left, right);
                break;

            case ExpressionType.Equal:
                builder.AppendFormat ("({0} == {1})", left, right);
                break;

            case ExpressionType.ExclusiveOr:
                builder.AppendFormat ("({0} ^ {1})", left, right);
                break;

            case ExpressionType.GreaterThan:
                builder.AppendFormat ("({0} > {1})", left, right);
                break;

            case ExpressionType.GreaterThanOrEqual:
                builder.AppendFormat ("({0} >= {1})", left, right);
                break;

            case ExpressionType.LeftShift:
                builder.AppendFormat ("({0} << {1})", left, right);
                break;

            case ExpressionType.LessThan:
                builder.AppendFormat ("({0} < {1})", left, right);
                break;

            case ExpressionType.LessThanOrEqual:
                builder.AppendFormat ("({0} <= {1})", left, right);
                break;

            case ExpressionType.Modulo:
                builder.AppendFormat ("({0} % {1})", left, right);
                break;

            case ExpressionType.Multiply:
                builder.AppendFormat ("({0} * {1})", left, right);
                break;

            case ExpressionType.MultiplyChecked:
                builder.AppendFormat ("({0} * {1})", left, right);
                break;

            case ExpressionType.NotEqual:
                builder.AppendFormat ("({0} != {1})", left, right);
                break;

            // See below for ExpressionType.Or.

            case ExpressionType.OrElse:
                builder.AppendFormat ("({0} || {1})", left, right);
                break;

            case ExpressionType.RightShift:
                builder.AppendFormat ("({0} >> {1})", left, right);
                break;

            case ExpressionType.Subtract:
                builder.AppendFormat ("({0} - {1})", left, right);
                break;

            case ExpressionType.SubtractChecked:
                builder.AppendFormat ("({0} - {1})", left, right);
                break;

            // 'ExpressionType.And' and 'ExpressionType.Or' are special because
            // when the arguments' type is a bool the operator changes from '&'
            // or '|' to 'And' or 'Or'.
            // FIXME: is this correct or it is an error in MS implementation?
            
            case ExpressionType.And:
                if (Type == typeof(bool))
                    builder.AppendFormat ("({0} And {1})", left, right);                
                else
                    builder.AppendFormat ("({0} & {1})", left, right);
                break;

            case ExpressionType.Or:
                if (Type == typeof(bool))
                    builder.AppendFormat ("({0} Or {1})", left, right);                
                else
                    builder.AppendFormat ("({0} | {1})", left, right);
                break;
            }
        }
        #endregion
    }
}