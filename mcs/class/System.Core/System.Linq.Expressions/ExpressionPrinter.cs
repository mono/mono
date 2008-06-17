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

		const string ListSeparator = ", ";

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
			Print (initializer.AddMethod);
			Print ("(");
			VisitExpressionList (initializer.Arguments);
			Print (")");
		}

		protected override void VisitUnary (UnaryExpression unary)
		{
			switch (unary.NodeType) {
			case ExpressionType.ArrayLength:
			case ExpressionType.Convert:
			case ExpressionType.ConvertChecked:
			case ExpressionType.Not:
				Print ("{0}(", unary.NodeType);
				Visit (unary.Operand);
				Print (")");
				return;
			case ExpressionType.Negate:
				Print ("-");
				Visit (unary.Operand);
				return;
			case ExpressionType.Quote:
				Visit (unary.Operand);
				return;
			case ExpressionType.TypeAs:
				Print ("(");
				Visit (unary.Operand);
				Print (" As {0})", unary.Type.Name);
				return;
			case ExpressionType.UnaryPlus:
				Print ("+");
				Visit (unary.Operand);
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
				return "=";
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

		void PrintArrayIndex (BinaryExpression index)
		{
			Visit (index.Left);
			Print ("[");
			Visit (index.Right);
			Print ("]");
		}

		protected override void VisitBinary (BinaryExpression binary)
		{
			switch (binary.NodeType) {
			case ExpressionType.ArrayIndex:
				PrintArrayIndex (binary);
				return;
			default:
				Print ("(");
				Visit (binary.Left);
				Print (" {0} ", OperatorToString (binary));
				Visit (binary.Right);
				Print (")");
				return;
			}
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
			Print ("IIF(");
			Visit (conditional.Test);
			Print (ListSeparator);
			Visit (conditional.IfTrue);
			Print (ListSeparator);
			Visit (conditional.IfFalse);
			Print (")");
		}

		protected override void VisitParameter (ParameterExpression parameter)
		{
			Print (parameter.Name ?? "<param>");
		}

		protected override void VisitMemberAccess (MemberExpression access)
		{
			if (access.Expression == null)
				Print (access.Member.DeclaringType.Name);
			else
				Visit (access.Expression);

			Print (".{0}", access.Member.Name);
		}

		protected override void VisitMethodCall (MethodCallExpression call)
		{
			if (call.Object != null) {
				Visit (call.Object);
				Print (".");
			}
			Print (call.Method.Name);
			Print ("(");
			VisitExpressionList (call.Arguments);
			Print (")");
		}

		protected override void VisitMemberAssignment (MemberAssignment assignment)
		{
			Print ("{0} = ", assignment.Member.Name);
			Visit (assignment.Expression);
		}

		protected override void VisitMemberMemberBinding (MemberMemberBinding binding)
		{
			Print (binding.Member.Name);
			Print (" = {");
			// VisitBindingList (binding.Bindings);
			VisitList (binding.Bindings, VisitBinding);
			Print ("}");
		}

		protected override void VisitMemberListBinding (MemberListBinding binding)
		{
			Print (binding.Member.Name);
			Print (" = {");
			// replace when the patch to the visitor is in
			// VisitElementInitializerList (binding.Initializers);
			VisitList (binding.Initializers, VisitElementInitializer);
			Print ("}");
		}

		protected override void VisitList<T> (ReadOnlyCollection<T> list, Action<T> visitor)
		{
			for (int i = 0; i < list.Count; i++) {
				if (i > 0)
					Print (ListSeparator);

				visitor (list [i]);
			}
		}

		protected override void VisitLambda (LambdaExpression lambda)
		{
			if (lambda.Parameters.Count != 1) {
				Print ("(");
				// replace when the patch to the visitor is in
				// VisitExpressionList (lambda.Parameters);
				VisitList (lambda.Parameters, Visit);
				Print (")");
			} else
				Visit (lambda.Parameters [0]);

			Print (" => ");
			Visit (lambda.Body);
		}

		protected override void VisitNew (NewExpression nex)
		{
			Print ("new {0}(", nex.Type.Name);
			if (nex.Members != null && nex.Members.Count > 0) {
				for (int i = 0; i < nex.Members.Count; i++) {
					if (i > 0)
						Print (ListSeparator);

					Print ("{0} = ", nex.Members [i].Name);
					Visit (nex.Arguments [i]);
				}
			} else
				VisitExpressionList (nex.Arguments);
			Print (")");
		}

		protected override void VisitMemberInit (MemberInitExpression init)
		{
			Visit (init.NewExpression);
			Print (" {");
			// VisitBindingList (init.Bindings)
			VisitList (init.Bindings, VisitBinding);
			Print ("}");
		}

		protected override void VisitListInit (ListInitExpression init)
		{
			Visit (init.NewExpression);
			Print (" {");
			// VisitElementInitializerList
			VisitList (init.Initializers, VisitElementInitializer);
			Print ("}");
		}

		protected override void VisitNewArray (NewArrayExpression newArray)
		{
			Print ("new ");
			switch (newArray.NodeType) {
			case ExpressionType.NewArrayBounds:
				Print (newArray.Type);
				Print ("(");
				VisitExpressionList (newArray.Expressions);
				Print (")");
				return;
			case ExpressionType.NewArrayInit:
				Print ("[] {");
				VisitExpressionList (newArray.Expressions);
				Print ("}");
				return;
			}

			throw new NotSupportedException ();
		}

		protected override void VisitInvocation (InvocationExpression invocation)
		{
			Print ("Invoke(");
			Visit (invocation.Expression);

			if (invocation.Arguments.Count != 0) {
				Print (ListSeparator);
				VisitExpressionList (invocation.Arguments);
			}

			Print (")");
		}
	}
}
