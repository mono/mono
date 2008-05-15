//
// ExpressionVisitor.cs
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
using System.Collections.ObjectModel;

namespace System.Linq.Expressions {

	abstract class ExpressionVisitor {

		protected virtual void Visit (Expression expression)
		{
			if (expression == null)
				return;

			switch (expression.NodeType) {
			case ExpressionType.Negate:
			case ExpressionType.NegateChecked:
			case ExpressionType.Not:
			case ExpressionType.Convert:
			case ExpressionType.ConvertChecked:
			case ExpressionType.ArrayLength:
			case ExpressionType.Quote:
			case ExpressionType.TypeAs:
			case ExpressionType.UnaryPlus:
				VisitUnary ((UnaryExpression) expression);
				break;
			case ExpressionType.Add:
			case ExpressionType.AddChecked:
			case ExpressionType.Subtract:
			case ExpressionType.SubtractChecked:
			case ExpressionType.Multiply:
			case ExpressionType.MultiplyChecked:
			case ExpressionType.Divide:
			case ExpressionType.Modulo:
			case ExpressionType.Power:
			case ExpressionType.And:
			case ExpressionType.AndAlso:
			case ExpressionType.Or:
			case ExpressionType.OrElse:
			case ExpressionType.LessThan:
			case ExpressionType.LessThanOrEqual:
			case ExpressionType.GreaterThan:
			case ExpressionType.GreaterThanOrEqual:
			case ExpressionType.Equal:
			case ExpressionType.NotEqual:
			case ExpressionType.Coalesce:
			case ExpressionType.ArrayIndex:
			case ExpressionType.RightShift:
			case ExpressionType.LeftShift:
			case ExpressionType.ExclusiveOr:
				VisitBinary ((BinaryExpression) expression);
				break;
			case ExpressionType.TypeIs:
				VisitTypeIs ((TypeBinaryExpression) expression);
				break;
			case ExpressionType.Conditional:
				VisitConditional ((ConditionalExpression) expression);
				break;
			case ExpressionType.Constant:
				VisitConstant ((ConstantExpression) expression);
				break;
			case ExpressionType.Parameter:
				VisitParameter ((ParameterExpression) expression);
				break;
			case ExpressionType.MemberAccess:
				VisitMemberAccess ((MemberExpression) expression);
				break;
			case ExpressionType.Call:
				VisitMethodCall ((MethodCallExpression) expression);
				break;
			case ExpressionType.Lambda:
				VisitLambda ((LambdaExpression) expression);
				break;
			case ExpressionType.New:
				VisitNew ((NewExpression) expression);
				break;
			case ExpressionType.NewArrayInit:
			case ExpressionType.NewArrayBounds:
				VisitNewArray ((NewArrayExpression) expression);
				break;
			case ExpressionType.Invoke:
				VisitInvocation ((InvocationExpression) expression);
				break;
			case ExpressionType.MemberInit:
				VisitMemberInit ((MemberInitExpression) expression);
				break;
			case ExpressionType.ListInit:
				VisitListInit ((ListInitExpression) expression);
				break;
			default:
				throw new ArgumentException (string.Format ("Unhandled expression type: '{0}'", expression.NodeType));
			}
		}

		protected virtual void VisitBinding (MemberBinding binding)
		{
			switch (binding.BindingType) {
			case MemberBindingType.Assignment:
				VisitMemberAssignment ((MemberAssignment) binding);
				break;
			case MemberBindingType.MemberBinding:
				VisitMemberMemberBinding ((MemberMemberBinding) binding);
				break;
			case MemberBindingType.ListBinding:
				VisitMemberListBinding ((MemberListBinding) binding);
				break;
			default:
				throw new ArgumentException (string.Format ("Unhandled binding type '{0}'", binding.BindingType));
			}
		}

		protected virtual void VisitElementInitializer (ElementInit initializer)
		{
			VisitExpressionList (initializer.Arguments);
		}

		protected virtual void VisitUnary (UnaryExpression unary)
		{
			Visit (unary.Operand);
		}

		protected virtual void VisitBinary (BinaryExpression binary)
		{
			Visit (binary.Left);
			Visit (binary.Right);
			Visit (binary.Conversion);
		}

		protected virtual void VisitTypeIs (TypeBinaryExpression type)
		{
			Visit (type.Expression);
		}

		protected virtual void VisitConstant (ConstantExpression constant)
		{
		}

		protected virtual void VisitConditional (ConditionalExpression conditional)
		{
			Visit (conditional.Test);
			Visit (conditional.IfTrue);
			Visit (conditional.IfFalse);
		}

		protected virtual void VisitParameter (ParameterExpression parameter)
		{
		}

		protected virtual void VisitMemberAccess (MemberExpression member)
		{
			Visit (member.Expression);
		}

		protected virtual void VisitMethodCall (MethodCallExpression methodCall)
		{
			Visit (methodCall.Object);
			VisitExpressionList (methodCall.Arguments);
		}

		protected virtual void VisitList<T> (ReadOnlyCollection<T> list, Action<T> visitor)
		{
			foreach (T element in list) {
				visitor (element);
			}
		}

		protected virtual void VisitExpressionList (ReadOnlyCollection<Expression> list)
		{
			VisitList (list, Visit);
		}

		protected virtual void VisitMemberAssignment (MemberAssignment assignment)
		{
			Visit (assignment.Expression);
		}

		protected virtual void VisitMemberMemberBinding (MemberMemberBinding binding)
		{
			VisitBindingList (binding.Bindings);
		}

		protected virtual void VisitMemberListBinding (MemberListBinding binding)
		{
			VisitElementInitializerList (binding.Initializers);
		}

		protected virtual void VisitBindingList (ReadOnlyCollection<MemberBinding> list)
		{
			VisitList (list, VisitBinding);
		}

		protected virtual void VisitElementInitializerList (ReadOnlyCollection<ElementInit> list)
		{
			VisitList (list, VisitElementInitializer);
		}

		protected virtual void VisitLambda (LambdaExpression lambda)
		{
			Visit (lambda.Body);
		}

		protected virtual void VisitNew (NewExpression nex)
		{
			VisitExpressionList (nex.Arguments);
		}

		protected virtual void VisitMemberInit (MemberInitExpression init)
		{
			VisitNew (init.NewExpression);
			VisitBindingList (init.Bindings);
		}

		protected virtual void VisitListInit (ListInitExpression init)
		{
			VisitNew (init.NewExpression);
			VisitElementInitializerList (init.Initializers);
		}

		protected virtual void VisitNewArray (NewArrayExpression newArray)
		{
			VisitExpressionList (newArray.Expressions);
		}

		protected virtual void VisitInvocation (InvocationExpression invocation)
		{
			VisitExpressionList (invocation.Arguments);
			Visit (invocation.Expression);
		}
	}
}
