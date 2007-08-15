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
		static void AppendFormat (StringBuilder sb, string format, params object [] args)
		{
			sb.AppendFormat (format, args);
		}
		
        internal override void BuildString(StringBuilder builder)
        {
            switch (NodeType)
            {
            case ExpressionType.Add:
                AppendFormat (builder, "({0} + {1})", left, right);
                break;
                
            case ExpressionType.AddChecked:
                AppendFormat (builder, "({0} + {1})", left, right);
                break;

            // See below for ExpressionType.And.
            
            case ExpressionType.AndAlso:
                AppendFormat (builder, "({0} && {1})", left, right);
                break;
            
            case ExpressionType.ArrayIndex:
                AppendFormat (builder, "{0}[{1}]", left, right);
                break;

            case ExpressionType.Coalesce:
                AppendFormat (builder, "({0} ?? {1})", left, right);
                break;

            case ExpressionType.Divide:
                AppendFormat (builder, "({0} / {1})", left, right);
                break;

            case ExpressionType.Equal:
                AppendFormat (builder, "({0} == {1})", left, right);
                break;

            case ExpressionType.ExclusiveOr:
                AppendFormat (builder, "({0} ^ {1})", left, right);
                break;

            case ExpressionType.GreaterThan:
                AppendFormat (builder, "({0} > {1})", left, right);
                break;

            case ExpressionType.GreaterThanOrEqual:
                AppendFormat (builder, "({0} >= {1})", left, right);
                break;

            case ExpressionType.LeftShift:
                AppendFormat (builder, "({0} << {1})", left, right);
                break;

            case ExpressionType.LessThan:
                AppendFormat (builder, "({0} < {1})", left, right);
                break;

            case ExpressionType.LessThanOrEqual:
                AppendFormat (builder, "({0} <= {1})", left, right);
                break;

            case ExpressionType.Modulo:
                AppendFormat (builder, "({0} % {1})", left, right);
                break;

            case ExpressionType.Multiply:
                AppendFormat (builder, "({0} * {1})", left, right);
                break;

            case ExpressionType.MultiplyChecked:
                AppendFormat (builder, "({0} * {1})", left, right);
                break;

            case ExpressionType.NotEqual:
                AppendFormat (builder, "({0} != {1})", left, right);
                break;

            // See below for ExpressionType.Or.

            case ExpressionType.OrElse:
                AppendFormat (builder, "({0} ^ {1})", left, right);
                break;

            case ExpressionType.RightShift:
                AppendFormat (builder, "({0} >> {1})", left, right);
                break;

            case ExpressionType.Subtract:
                AppendFormat (builder, "({0} - {1})", left, right);
                break;

            case ExpressionType.SubtractChecked:
                AppendFormat (builder, "({0} - {1})", left, right);
                break;

            // 'ExpressionType.And' and 'ExpressionType.Or' are special because
            // when the arguments' type is a bool the operator changes from '&'
            // or '|' to 'And' or 'Or'.
            // FIXME: is this correct or it is an error in MS implementation?
            
            case ExpressionType.And:
                if (Type == typeof(bool))
                    AppendFormat (builder, "({0} And {1})", left, right);                
                else
                    AppendFormat (builder, "({0} & {1})", left, right);
                break;

            case ExpressionType.Or:
                if (Type == typeof(bool))
                    AppendFormat (builder, "({0} Or {1})", left, right);                
                else
                    AppendFormat (builder, "({0} | {1})", left, right);
                break;
            }
        }
        #endregion
    }
}