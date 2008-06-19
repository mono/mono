//
// ExpressionTransformer.cs
//
// Authors:
//	Roei Erez (roeie@mainsoft.com)
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace System.Linq.Expressions {

	abstract class ExpressionTransformer {

		public Expression Transform (Expression e)
		{
			return Visit (e);
		}

		protected virtual Expression Visit (Expression expression)
		{
			if (expression == null)
				return null;

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
				return VisitUnary ((UnaryExpression) expression);
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
				return VisitBinary ((BinaryExpression) expression);
			case ExpressionType.TypeIs:
				return VisitTypeIs ((TypeBinaryExpression) expression);
			case ExpressionType.Conditional:
				return VisitConditional ((ConditionalExpression) expression);
			case ExpressionType.Constant:
				return VisitConstant ((ConstantExpression) expression);
			case ExpressionType.Parameter:
				return VisitParameter ((ParameterExpression) expression);
			case ExpressionType.MemberAccess:
				return VisitMemberAccess ((MemberExpression) expression);
			case ExpressionType.Call:
				return VisitMethodCall ((MethodCallExpression) expression);
			case ExpressionType.Lambda:
				return VisitLambda ((LambdaExpression) expression);
			case ExpressionType.New:
				return VisitNew ((NewExpression) expression);
			case ExpressionType.NewArrayInit:
			case ExpressionType.NewArrayBounds:
				return VisitNewArray ((NewArrayExpression) expression);
			case ExpressionType.Invoke:
				return VisitInvocation ((InvocationExpression) expression);
			case ExpressionType.MemberInit:
				return VisitMemberInit ((MemberInitExpression) expression);
			case ExpressionType.ListInit:
				return VisitListInit ((ListInitExpression) expression);
			default:
				throw new ArgumentException (string.Format ("Unhandled expression type: '{0}'", expression.NodeType));
			}
		}

		protected virtual MemberBinding VisitBinding (MemberBinding binding)
		{
			switch (binding.BindingType) {
			case MemberBindingType.Assignment:
				return VisitMemberAssignment ((MemberAssignment) binding);
			case MemberBindingType.MemberBinding:
				return VisitMemberMemberBinding ((MemberMemberBinding) binding);
			case MemberBindingType.ListBinding:
				return VisitMemberListBinding ((MemberListBinding) binding);
			default:
				throw new ArgumentException (string.Format ("Unhandled binding type '{0}'", binding.BindingType));
			}
		}

		protected virtual ElementInit VisitElementInitializer (ElementInit initializer)
		{
			ReadOnlyCollection<Expression> transformed = VisitExpressionList (initializer.Arguments);
			if (transformed != initializer.Arguments)
				return Expression.ElementInit (initializer.AddMethod, transformed);
			return initializer;
		}

		protected virtual UnaryExpression VisitUnary (UnaryExpression unary)
		{
			Expression transformedOperand = Visit (unary.Operand);
			if (transformedOperand != unary.Operand)
				return Expression.MakeUnary (unary.NodeType, transformedOperand, unary.Type, unary.Method);
			return unary;
		}

		protected virtual BinaryExpression VisitBinary (BinaryExpression binary)
		{
			Expression left = Visit (binary.Left);
			Expression right = Visit (binary.Right);
			LambdaExpression conversion = VisitLambda (binary.Conversion);
			if (left != binary.Left || right != binary.Right || conversion != binary.Conversion)
				return Expression.MakeBinary (binary.NodeType, left, right, binary.IsLiftedToNull, binary.Method, conversion);
			return binary;
		}

		protected virtual TypeBinaryExpression VisitTypeIs (TypeBinaryExpression type)
		{
			Expression inner = Visit (type.Expression);
			if (inner != type.Expression)
				return Expression.TypeIs (inner, type.TypeOperand);
			return type;
		}

		protected virtual ConstantExpression VisitConstant (ConstantExpression constant)
		{
			return constant;
		}

		protected virtual ConditionalExpression VisitConditional (ConditionalExpression conditional)
		{
			Expression test = Visit (conditional.Test);
			Expression ifTrue = Visit (conditional.IfTrue);
			Expression ifFalse = Visit (conditional.IfFalse);
			if (test != conditional.Test || ifTrue != conditional.IfTrue || ifFalse != conditional.IfFalse)
				return Expression.Condition (test, ifTrue, ifFalse);
			return conditional;
		}

		protected virtual ParameterExpression VisitParameter (ParameterExpression parameter)
		{
			return parameter;
		}

		protected virtual MemberExpression VisitMemberAccess (MemberExpression member)
		{
			Expression memberExp = Visit (member.Expression);
			if (memberExp != member.Expression)
				return Expression.MakeMemberAccess (memberExp, member.Member);
			return member;
		}

		protected virtual MethodCallExpression VisitMethodCall (MethodCallExpression methodCall)
		{
			Expression instance = Visit (methodCall.Object);
			ReadOnlyCollection<Expression> args = VisitExpressionList (methodCall.Arguments);
			if (instance != methodCall.Object || args != methodCall.Arguments)
				return Expression.Call (instance, methodCall.Method, args);
			return methodCall;
		}

		protected virtual ReadOnlyCollection<Expression> VisitExpressionList (ReadOnlyCollection<Expression> list)
		{
			return VisitList<Expression> (list, Visit);
		}

		private ReadOnlyCollection<T> VisitList<T> (ReadOnlyCollection<T> list, Func<T,T> selector) where T :class
		{
			int index = 0;
			T [] arr = null;
			foreach (T e in list) {
				T visited = selector (e);
				if (visited != e || arr != null) {
					if (arr == null)
						arr = new T [list.Count];
					arr [index] = visited;
				}
				index++;
			}
			if (arr != null)
				return arr.ToReadOnlyCollection ();
			return list;
		}

		protected virtual MemberAssignment VisitMemberAssignment (MemberAssignment assignment)
		{
			Expression inner = Visit (assignment.Expression);
			if (inner != assignment.Expression)
				return Expression.Bind (assignment.Member, inner);
			return assignment;
		}

		protected virtual MemberMemberBinding VisitMemberMemberBinding (MemberMemberBinding binding)
		{
			ReadOnlyCollection<MemberBinding> bindingExp = VisitBindingList (binding.Bindings);
			if (bindingExp != binding.Bindings)
				return Expression.MemberBind (binding.Member, bindingExp);
			return binding;
		}

		protected virtual MemberListBinding VisitMemberListBinding (MemberListBinding binding)
		{
			ReadOnlyCollection<ElementInit> initializers =
				VisitElementInitializerList (binding.Initializers);
			if (initializers != binding.Initializers)
				return Expression.ListBind (binding.Member, initializers);
			return binding;
		}

		protected virtual ReadOnlyCollection<MemberBinding> VisitBindingList (ReadOnlyCollection<MemberBinding> list)
		{
			return VisitList<MemberBinding> (list, VisitBinding);
		}

		protected virtual ReadOnlyCollection<ElementInit> VisitElementInitializerList (ReadOnlyCollection<ElementInit> list)
		{
			return VisitList<ElementInit> (list, VisitElementInitializer);
		}

		protected virtual LambdaExpression VisitLambda (LambdaExpression lambda)
		{
			Expression body = Visit (lambda.Body);
			ReadOnlyCollection<ParameterExpression> parameters =
				VisitList<ParameterExpression> (lambda.Parameters, VisitParameter);
			if (body != lambda.Body || parameters != lambda.Parameters)
				return Expression.Lambda (body, parameters.ToArray());
			return lambda;
		}

		protected virtual NewExpression VisitNew (NewExpression nex)
		{
			ReadOnlyCollection<Expression> args = VisitList (nex.Arguments, Visit);
			if (args != nex.Arguments)
				return Expression.New (nex.Constructor, args);
			return nex;
		}

		protected virtual MemberInitExpression VisitMemberInit (MemberInitExpression init)
		{
			NewExpression  newExp = VisitNew (init.NewExpression);
			ReadOnlyCollection<MemberBinding> bindings = VisitBindingList (init.Bindings);
			if (newExp != init.NewExpression || bindings != init.Bindings)
				return Expression.MemberInit (newExp, bindings);
			return init;
		}

		protected virtual ListInitExpression VisitListInit (ListInitExpression init)
		{
			NewExpression newExp = VisitNew (init.NewExpression);
			ReadOnlyCollection<ElementInit> initializers = VisitElementInitializerList (init.Initializers);
			if (newExp != init.NewExpression || initializers != init.Initializers)
				return Expression.ListInit (newExp, initializers.ToArray());
			return init;
		}

		protected virtual NewArrayExpression VisitNewArray (NewArrayExpression newArray)
		{
			ReadOnlyCollection<Expression> expressions = VisitExpressionList (newArray.Expressions);
			if (expressions != newArray.Expressions) {
				if (newArray.NodeType == ExpressionType.NewArrayBounds)
					return Expression.NewArrayBounds (newArray.Type, expressions);
				else
					return Expression.NewArrayInit (newArray.Type, expressions);
			}
			return newArray;
		}

		protected virtual InvocationExpression VisitInvocation (InvocationExpression invocation)
		{
			ReadOnlyCollection<Expression> args = VisitExpressionList (invocation.Arguments);
			Expression invocationExp = Visit (invocation.Expression);
			if (args != invocation.Arguments || invocationExp != invocation.Expression)
				return Expression.Invoke (invocationExp, args);
			return invocation;
		}
	}
}
