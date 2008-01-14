//
// ExpressionPrinter.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// (C) 2008 Novell, Inc. (http://www.novell.com)
//
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
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace System.Linq.Expressions {

	class ExpressionPrinter : ExpressionVisitor {

		StringBuilder builder;

		ExpressionPrinter (StringBuilder builder)
		{
			this.builder = builder;
		}

		ExpressionPrinter () : this (new StringBuilder ())
		{
		}

		public static string ToString (Expression expression)
		{
			var printer = new ExpressionPrinter ();
			printer.Visit (expression);
			return printer.builder.ToString ();
		}

		public static string ToString (ElementInit init)
		{
			var printer = new ExpressionPrinter ();
			printer.VisitElementInitializer (init);
			return printer.builder.ToString ();
		}

		public static string ToString (MemberBinding binding)
		{
			var printer = new ExpressionPrinter ();
			printer.VisitBinding (binding);
			return printer.builder.ToString ();
		}

		void Print (string str)
		{
			builder.Append (str);
		}

		void Print (object obj)
		{
			builder.Append (obj);
		}

		void Print (string str, params object [] objs)
		{
			builder.AppendFormat (str, objs);
		}

		protected override void VisitElementInitializer (ElementInit initializer)
		{
			throw new NotImplementedException ();
		}

		protected override void VisitUnary (UnaryExpression unary)
		{
			switch (unary.NodeType) {
			case ExpressionType.ArrayLength:
				Print ("{0}(", unary.NodeType);
				Visit (unary.Operand);
				Print (")");
				return;
			case ExpressionType.Quote:
				Visit (unary.Operand);
				return;
			case ExpressionType.TypeAs:
				Print ("(");
				Visit (unary.Operand);
				Print (" As {0})", unary.Type.Name);
				return;
			}

			throw new NotImplementedException ();
		}

		static string OperatorToString (BinaryExpression binary)
		{
			switch (binary.NodeType) {
			case ExpressionType.Add:
			case ExpressionType.AddChecked:
				return "+";
			case ExpressionType.AndAlso:
				return "&&";
			case ExpressionType.Coalesce:
				return "??";
			case ExpressionType.Divide:
				return "/";
			case ExpressionType.Equal:
				return "==";
			case ExpressionType.ExclusiveOr:
				return "^";
			case ExpressionType.GreaterThan:
				return ">";
			case ExpressionType.GreaterThanOrEqual:
				return ">=";
			case ExpressionType.LeftShift:
				return "<<";
			case ExpressionType.LessThan:
				return "<";
			case ExpressionType.LessThanOrEqual:
				return "<=";
			case ExpressionType.Modulo:
				return "%";
			case ExpressionType.Multiply:
			case ExpressionType.MultiplyChecked:
				return "*";
			case ExpressionType.NotEqual:
				return "!=";
			case ExpressionType.OrElse:
				return "||";
			case ExpressionType.Power:
				return "^";
			case ExpressionType.RightShift:
				return ">>";
			case ExpressionType.Subtract:
			case ExpressionType.SubtractChecked:
				return "-";
			case ExpressionType.And:
				return IsBoolean (binary) ? "And" : "&";
			case ExpressionType.Or:
				return IsBoolean (binary) ? "Or" : "|";
			default:
				return null;
			}
		}

		static bool IsBoolean (Expression expression)
		{
			return expression.Type == typeof (bool) || expression.Type == typeof (bool?);
		}

		protected override void VisitBinary (BinaryExpression binary)
		{
			throw new NotImplementedException ();
		}

		protected override void VisitTypeIs (TypeBinaryExpression type)
		{
			switch (type.NodeType) {
			case ExpressionType.TypeIs:
				Print ("(");
				Visit (type.Expression);
				Print (" Is {0})", type.TypeOperand.Name);
				return;
			}

			throw new NotImplementedException ();
		}

		protected override void VisitConstant (ConstantExpression constant)
		{
			var value = constant.Value;

			if (value == null) {
				Print ("null");
			} else if (value is string) {
				Print ("\"");
				Print (value);
				Print ("\"");
			} else if (!HasStringRepresentation (value)) {
				Print ("value(");
				Print (value);
				Print (")");
			} else
				Print (value);
		}

		static bool HasStringRepresentation (object obj)
		{
			return obj.ToString () != obj.GetType ().ToString ();
		}

		protected override void VisitConditional (ConditionalExpression conditional)
		{
			throw new NotImplementedException ();
		}

		protected override void VisitParameter (ParameterExpression parameter)
		{
			throw new NotImplementedException ();
		}

		protected override void VisitMemberAccess (MemberExpression member)
		{
			throw new NotImplementedException ();
		}

		protected override void VisitMethodCall (MethodCallExpression methodCall)
		{
			throw new NotImplementedException ();
		}

		protected override void VisitExpressionList (ReadOnlyCollection<Expression> list)
		{
			throw new NotImplementedException ();
		}

		protected override void VisitMemberAssignment (MemberAssignment assignment)
		{
			throw new NotImplementedException ();
		}

		protected override void VisitMemberMemberBinding (MemberMemberBinding binding)
		{
			throw new NotImplementedException ();
		}

		protected override void VisitMemberListBinding (MemberListBinding binding)
		{
			throw new NotImplementedException ();
		}

		protected override void VisitList<T> (ReadOnlyCollection<T> list, Action<T> visitor)
		{
			for (int i = 0; i < list.Count; i++) {
				if (i > 0)
					Print (", ");

				visitor (list [i]);
			}
		}

		protected override void VisitLambda (LambdaExpression lambda)
		{
			throw new NotImplementedException ();
		}

		protected override void VisitNew (NewExpression nex)
		{
			throw new NotImplementedException ();
		}

		protected override void VisitMemberInit (MemberInitExpression init)
		{
			throw new NotImplementedException ();
		}

		protected override void VisitListInit (ListInitExpression init)
		{
			throw new NotImplementedException ();
		}

		protected override void VisitNewArray (NewArrayExpression newArray)
		{
			throw new NotImplementedException ();
		}

		protected override void VisitInvocation (InvocationExpression invocation)
		{
			throw new NotImplementedException ();
		}
	}
}
