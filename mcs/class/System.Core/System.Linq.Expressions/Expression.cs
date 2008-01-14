//
// Expression.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//   Miguel de Icaza (miguel@novell.com)
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
using System.Reflection;

namespace System.Linq.Expressions {

	public abstract class Expression {

		ExpressionType node_type;
		Type type;

		public ExpressionType NodeType {
			get { return node_type; }
		}

		public Type Type {
			get { return type; }
		}

		// TODO: remove when all Expression subtypes
		// have their constructor implemented
		protected Expression ()
		{
		}

		protected Expression (ExpressionType node_type, Type type)
		{
			this.node_type = node_type;
			this.type = type;
		}

		public override string ToString ()
		{
			return ExpressionPrinter.ToString (this);
		}

		public static BinaryExpression Add (Expression left, Expression right)
		{
			return MakeBinary (ExpressionType.Add, left, right);
		}

		public static BinaryExpression Add (Expression left, Expression right, MethodInfo method)
		{
			return MakeBinary (ExpressionType.Add, left, right, false, method);
		}

		public static BinaryExpression AddChecked (Expression left, Expression right)
		{
			return MakeBinary (ExpressionType.AddChecked, left, right);
		}

		public static BinaryExpression AddChecked (Expression left, Expression right, MethodInfo method)
		{
			return MakeBinary (ExpressionType.AddChecked, left, right, false, method);
		}

		public static BinaryExpression And (Expression left, Expression right)
		{
			return MakeBinary (ExpressionType.And, left, right);
		}

		public static BinaryExpression And (Expression left, Expression right, MethodInfo method)
		{
			return MakeBinary (ExpressionType.And, left, right, false, method);
		}

		public static BinaryExpression AndAlso (Expression left, Expression right)
		{
			return MakeBinary (ExpressionType.AndAlso, left, right);
		}

		public static BinaryExpression AndAlso (Expression left, Expression right, MethodInfo method)
		{
			return MakeBinary (ExpressionType.AndAlso, left, right, false, method);
		}

		[MonoTODO]
		public static MethodCallExpression ArrayIndex (Expression left, params Expression [] indexes)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MethodCallExpression ArrayIndex (Expression left, IEnumerable<Expression> indexes)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static BinaryExpression ArrayIndex (Expression left, Expression index)
		{
			throw new NotImplementedException ();
		}

		public static UnaryExpression ArrayLength (Expression array)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (!array.Type.IsArray)
				throw new ArgumentException ("The type of the expression must me Array");
			if (array.Type.GetArrayRank () != 1)
				throw new ArgumentException ("The array must be a single dimensional array");

			return new UnaryExpression (ExpressionType.ArrayLength, array, typeof (int));
		}

		[MonoTODO]
		public static MemberAssignment Bind (MemberInfo member, Expression expression)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberAssignment Bind (MethodInfo propertyAccessor, Expression expression)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MethodCallExpression Call (Expression instance, MethodInfo method)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MethodCallExpression Call (MethodInfo method, params Expression [] arguments)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MethodCallExpression Call (Expression instance, MethodInfo method, params Expression [] arguments)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MethodCallExpression Call (Expression instance, MethodInfo method, IEnumerable<Expression> arguments)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MethodCallExpression Call (Expression instance, string methodName, Type [] typeArguments, params Expression [] arguments)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MethodCallExpression Call (Type type, string methodName, Type [] typeArguments, params Expression [] arguments)
		{
			throw new NotImplementedException ();
		}

		public static BinaryExpression Coalesce (Expression left, Expression right)
		{
			return MakeBinary (ExpressionType.Coalesce, left, right);
		}

		public static BinaryExpression Coalesce (Expression left, Expression right, LambdaExpression conversion)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ConditionalExpression Condition (Expression test, Expression ifTrue, Expression ifFalse)
		{
			throw new NotImplementedException ();
		}

		public static ConstantExpression Constant (object value)
		{
			if (value == null)
				return new ConstantExpression (null, typeof (object));
			
			return Constant (value, value.GetType ());
		}

		public static ConstantExpression Constant (object value, Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			//
			// value must be compatible with type, no conversions
			// are allowed
			//
			if (value == null){
				if (type.IsValueType && !IsNullable (type))
					throw new ArgumentException ();
			} else {
				if (!(type.IsValueType && IsNullable (type)) && value.GetType () != type)
					throw new ArgumentException ();
					
			}

			return new ConstantExpression (value, type);
		}

		[MonoTODO]
		public static UnaryExpression Convert (Expression expression, Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static UnaryExpression Convert (Expression expression, Type type, MethodInfo method)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static UnaryExpression ConvertChecked (Expression expression, Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static UnaryExpression ConvertChecked (Expression expression, Type type, MethodInfo method)
		{
			throw new NotImplementedException ();
		}

		public static BinaryExpression Divide (Expression left, Expression right)
		{
			return MakeBinary (ExpressionType.Divide, left, right);
		}

		public static BinaryExpression Divide (Expression left, Expression right, MethodInfo method)
		{
			return MakeBinary (ExpressionType.Divide, left, right, false, method);
		}

		[MonoTODO]
		public static ElementInit ElementInit (MethodInfo addMethod, params Expression [] arguments)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ElementInit ElementInit (MethodInfo addMethod, IEnumerable<Expression> arguments)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static BinaryExpression Equal (Expression left, Expression right)
		{
			return MakeBinary (ExpressionType.Equal, left, right);
		}

		public static BinaryExpression Equal (Expression left, Expression right, bool liftToNull, MethodInfo method)
		{
			BinaryExpression ret = MakeBinary (ExpressionType.Equal, left, right, liftToNull, method);

			return ret;
		}

		public static BinaryExpression ExclusiveOr (Expression left, Expression right)
		{
			return MakeBinary (ExpressionType.ExclusiveOr, left, right);
		}

		public static BinaryExpression ExclusiveOr (Expression left, Expression right, MethodInfo method)
		{
			return MakeBinary (ExpressionType.ExclusiveOr, left, right, false, method);
		}

		[MonoTODO]
		public static MemberExpression Field (Expression expression, FieldInfo field)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberExpression Field (Expression expression, string fieldName)
		{
			throw new NotImplementedException ();
		}

		public static Type GetActionType (params Type [] typeArgs)
		{
			if (typeArgs == null)
				throw new ArgumentNullException ("typeArgs");

			if (typeArgs.Length > 4)
				throw new ArgumentException ("No Action type of this arity");

			if (typeArgs.Length == 0)
				return typeof (Action);

			Type action = null;
			switch (typeArgs.Length) {
			case 1:
				action = typeof (Action<>);
				break;
			case 2:
				action = typeof (Action<,>);
				break;
			case 3:
				action = typeof (Action<,,>);
				break;
			case 4:
				action = typeof (Action<,,,>);
				break;
			}

			return action.MakeGenericType (typeArgs);
		}

		public static Type GetFuncType (params Type [] typeArgs)
		{
			if (typeArgs == null)
				throw new ArgumentNullException ("typeArgs");

			if (typeArgs.Length < 1 || typeArgs.Length > 5)
				throw new ArgumentException ("No Func type of this arity");

			Type func = null;
			switch (typeArgs.Length) {
			case 1:
				func = typeof (Func<>);
				break;
			case 2:
				func = typeof (Func<,>);
				break;
			case 3:
				func = typeof (Func<,,>);
				break;
			case 4:
				func = typeof (Func<,,,>);
				break;
			case 5:
				func = typeof (Func<,,,,>);
				break;
			}

			return func.MakeGenericType (typeArgs);
		}

		public static BinaryExpression GreaterThan (Expression left, Expression right)
		{
			return MakeBinary (ExpressionType.GreaterThan, left, right);
		}

		public static BinaryExpression GreaterThan (Expression left, Expression right, bool liftToNull, MethodInfo method)
		{
			return MakeBinary (ExpressionType.GreaterThan, left, right, liftToNull, method);
		}

		public static BinaryExpression GreaterThanOrEqual (Expression left, Expression right)
		{
			return MakeBinary (ExpressionType.GreaterThanOrEqual, left, right);
		}

		public static BinaryExpression GreaterThanOrEqual (Expression left, Expression right, bool liftToNull, MethodInfo method)
		{
			return MakeBinary (ExpressionType.GreaterThanOrEqual, left, right, liftToNull, method);
		}

		[MonoTODO]
		public static InvocationExpression Invoke (Expression expression, params Expression [] arguments)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static InvocationExpression Invoke (Expression expression, IEnumerable<Expression> arguments)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Expression<TDelegate> Lambda<TDelegate> (Expression body, params ParameterExpression [] parameters)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Expression<TDelegate> Lambda<TDelegate> (Expression body, IEnumerable<ParameterExpression> parameters)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static LambdaExpression Lambda (Expression body, params ParameterExpression [] parameters)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static LambdaExpression Lambda (Type delegateType, Expression body, params ParameterExpression [] parameters)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static LambdaExpression Lambda (Type delegateType, Expression body, IEnumerable<ParameterExpression> parameters)
		{
			throw new NotImplementedException ();
		}

		public static BinaryExpression LeftShift (Expression left, Expression right)
		{
			return MakeBinary (ExpressionType.LeftShift, left, right);
		}

		public static BinaryExpression LeftShift (Expression left, Expression right, MethodInfo method)
		{
			return MakeBinary (ExpressionType.LeftShift, left, right, false, method);
		}

		public static BinaryExpression LessThan (Expression left, Expression right)
		{
			return MakeBinary (ExpressionType.LessThan, left, right);
		}

		public static BinaryExpression LessThan (Expression left, Expression right, bool liftToNull, MethodInfo method)
		{
			return MakeBinary (ExpressionType.LessThan, left, right, liftToNull, method);
		}

		public static BinaryExpression LessThanOrEqual (Expression left, Expression right)
		{
			return MakeBinary (ExpressionType.LessThanOrEqual, left, right);
		}

		public static BinaryExpression LessThanOrEqual (Expression left, Expression right, bool liftToNull, MethodInfo method)
		{
			return MakeBinary (ExpressionType.LessThanOrEqual, left, right, liftToNull, method);
		}

		public static MemberListBinding ListBind (MemberInfo member, params ElementInit [] initializers)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberListBinding ListBind (MemberInfo member, IEnumerable<ElementInit> initializers)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberListBinding ListBind (MethodInfo propertyAccessor, params ElementInit [] initializers)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberListBinding ListBind (MethodInfo propertyAccessor, IEnumerable<ElementInit> initializers)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ListInitExpression ListInit (NewExpression newExpression, params ElementInit [] initializers)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ListInitExpression ListInit (NewExpression newExpression, IEnumerable<ElementInit> initializers)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ListInitExpression ListInit (NewExpression newExpression, params Expression [] initializers)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ListInitExpression ListInit (NewExpression newExpression, IEnumerable<Expression> initializers)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ListInitExpression ListInit (NewExpression newExpression, MethodInfo addMethod, params Expression [] initializers)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ListInitExpression ListInit (NewExpression newExpression, MethodInfo addMethod, IEnumerable<Expression> initializers)
		{
			throw new NotImplementedException ();
		}

		public static BinaryExpression MakeBinary (ExpressionType binaryType, Expression left, Expression right)
		{
			return MakeBinary (binaryType, left, right, false, null);
		}

		public static BinaryExpression MakeBinary (ExpressionType binaryType, Expression left, Expression right, bool liftToNull, MethodInfo method)
		{
			if (left == null)
				throw new ArgumentNullException ("left");
			if (right == null)
				throw new ArgumentNullException ("right");
			if (method == null)
				throw new ArgumentNullException ("method");

			if (binaryType < 0 || binaryType > ExpressionType.TypeIs)
				throw new ArgumentException ("Out of range", "binaryType");

			BinaryExpression ret = new BinaryExpression (left, right, liftToNull, method);
			ret.node_type = binaryType;

			return ret;
		}

		[MonoTODO]
		public static BinaryExpression MakeBinary (ExpressionType binaryType, Expression left, Expression right, bool liftToNull, MethodInfo method, LambdaExpression conversion)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberExpression MakeMemberAccess (Expression expression, MemberInfo member)
		{
			throw new NotImplementedException ();
		}

		public static UnaryExpression MakeUnary (ExpressionType unaryType, Expression operand, Type type)
		{
			return MakeUnary (unaryType, operand, null, null);
		}

		public static UnaryExpression MakeUnary (ExpressionType unaryType, Expression operand, Type type, MethodInfo method)
		{
			if (!IsUnaryOperator (unaryType))
				throw new ArgumentException ("Make unary expect an unary operator");
			if (operand == null)
				throw new ArgumentNullException ("operand");

			return new UnaryExpression (unaryType, operand, type != null ? type : operand.Type, method);
		}

		static bool IsUnaryOperator (ExpressionType type)
		{
			switch (type) {
			case ExpressionType.ArrayLength:
			case ExpressionType.Convert:
			case ExpressionType.ConvertChecked:
			case ExpressionType.Negate:
			case ExpressionType.NegateChecked:
			case ExpressionType.Not:
			case ExpressionType.Quote:
			case ExpressionType.TypeAs:
			case ExpressionType.UnaryPlus:
				return true;
			}

			return false;
		}

		[MonoTODO]
		public static MemberMemberBinding MemberBind (MemberInfo member, params MemberBinding [] binding)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberMemberBinding MemberBind (MemberInfo member, IEnumerable<MemberBinding> binding)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberMemberBinding MemberBind (MethodInfo propertyAccessor, params MemberBinding [] binding)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberMemberBinding MemberBind (MethodInfo propertyAccessor, IEnumerable<MemberBinding> binding)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberInitExpression MemberInit (NewExpression newExpression, params MemberBinding [] binding)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberInitExpression MemberInit (NewExpression newExpression, IEnumerable<MemberBinding> binding)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static BinaryExpression Modulo (Expression left, Expression right)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static BinaryExpression Modulo (Expression left, Expression right, MethodInfo method)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static BinaryExpression Multiply (Expression left, Expression right)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static BinaryExpression Multiply (Expression left, Expression right, MethodInfo method)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static BinaryExpression MultiplyChecked (Expression left, Expression right)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static BinaryExpression MultiplyChecked (Expression left, Expression right, MethodInfo method)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static UnaryExpression Negate (Expression expression)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static UnaryExpression Negate (Expression expression, MethodInfo method)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static UnaryExpression NegateChecked (Expression expression)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static UnaryExpression NegateChecked (Expression expression, MethodInfo method)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static NewExpression New (ConstructorInfo constructor)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static NewExpression New (Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static NewExpression New (ConstructorInfo constructor, params Expression [] arguments)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static NewExpression New (ConstructorInfo constructor, IEnumerable<Expression> arguments)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static NewExpression New (ConstructorInfo constructor, IEnumerable<Expression> arguments, params MemberInfo [] members)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static NewExpression New (ConstructorInfo constructor, IEnumerable<Expression> arguments, IEnumerable<MemberInfo> members)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static NewArrayExpression NewArrayBounds (Type type, params Expression [] bounds)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static NewArrayExpression NewArrayBounds (Type type, IEnumerable<Expression> bounds)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static NewArrayExpression NewArrayInit (Type type, params Expression [] bounds)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static NewArrayExpression NewArrayInit (Type type, IEnumerable<Expression> bounds)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static UnaryExpression Not (Expression expression)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static UnaryExpression Not (Expression expression, MethodInfo method)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static BinaryExpression NotEqual (Expression left, Expression right)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static BinaryExpression NotEqual (Expression left, Expression right, bool liftToNull, MethodInfo method)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static BinaryExpression Or (Expression left, Expression right)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static BinaryExpression Or (Expression left, Expression right, MethodInfo method)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static BinaryExpression OrElse (Expression left, Expression right)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static BinaryExpression OrElse (Expression left, Expression right, MethodInfo method)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ParameterExpression Parameter (Type type, string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static BinaryExpression Power (Expression left, Expression right)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static BinaryExpression Power (Expression left, Expression right, MethodInfo method)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberExpression Property (Expression expression, MethodInfo propertyAccessor)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberExpression Property (Expression expression, PropertyInfo property)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberExpression Property (Expression expression, string propertyName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberExpression PropertyOrField (Expression expression, string propertyOrFieldName)
		{
			throw new NotImplementedException ();
		}

		public static UnaryExpression Quote (Expression expression)
		{
			if (expression == null)
				throw new ArgumentNullException ("expression");

			return new UnaryExpression (ExpressionType.Quote, expression);
		}

		[MonoTODO]
		public static BinaryExpression RightShift (Expression left, Expression right)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static BinaryExpression RightShift (Expression left, Expression right, MethodInfo method)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static BinaryExpression Subtract (Expression left, Expression right)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static BinaryExpression Subtract (Expression left, Expression right, MethodInfo method)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static BinaryExpression SubtractChecked (Expression left, Expression right)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static BinaryExpression SubtractChecked (Expression left, Expression right, MethodInfo method)
		{
			throw new NotImplementedException ();
		}

		public static UnaryExpression TypeAs (Expression expression, Type type)
		{
			if (expression == null)
				throw new ArgumentNullException ("expression");
			if (type == null)
				throw new ArgumentNullException ("type");

			return new UnaryExpression (ExpressionType.TypeAs, expression, type);
		}

		[MonoTODO]
		public static TypeBinaryExpression TypeIs (Expression expression, Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static UnaryExpression UnaryPlus (Expression expression)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static UnaryExpression UnaryPlus (Expression expression, MethodInfo method)
		{
			throw new NotImplementedException ();
		}

		static bool IsNullable (Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition () == typeof (Nullable<>);
		}
	}
}
