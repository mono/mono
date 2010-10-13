//
// ExpressionInterpreter.cs
//
// (C) 2008 Mainsoft, Inc. (http://www.mainsoft.com)
// (C) 2008 db4objects, Inc. (http://www.db4o.com)
// (C) 2010 Novell, Inc. (http://www.novell.com)
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Linq.jvm {

	struct LambdaInfo {
		public readonly LambdaExpression Lambda;
		public readonly object [] Arguments;

		public LambdaInfo (LambdaExpression lambda, object [] arguments)
		{
			this.Lambda = lambda;
			this.Arguments = arguments;
		}
	}

	class HoistedVariableDetector : ExpressionVisitor {

		readonly Dictionary<ParameterExpression, LambdaExpression> parameter_to_lambda =
			new Dictionary<ParameterExpression, LambdaExpression> ();

		Dictionary<LambdaExpression, List<ParameterExpression>> hoisted_map;

		LambdaExpression lambda;

		public Dictionary<LambdaExpression, List<ParameterExpression>> Process (LambdaExpression lambda)
		{
			Visit (lambda);
			return hoisted_map;
		}

		protected override void VisitLambda (LambdaExpression lambda)
		{
			this.lambda = lambda;
			foreach (var parameter in lambda.Parameters)
				parameter_to_lambda [parameter] = lambda;
			base.VisitLambda (lambda);
		}

		protected override void VisitParameter (ParameterExpression parameter)
		{
			if (lambda.Parameters.Contains (parameter))
				return;

			Hoist (parameter);
		}

		void Hoist (ParameterExpression parameter)
		{
			LambdaExpression lambda;
			if (!parameter_to_lambda.TryGetValue (parameter, out lambda))
				return;

			if (hoisted_map == null)
				hoisted_map = new Dictionary<LambdaExpression, List<ParameterExpression>> ();

			List<ParameterExpression> hoisted;
			if (!hoisted_map.TryGetValue (lambda, out hoisted)) {
				hoisted = new List<ParameterExpression> ();
				hoisted_map [lambda] = hoisted;
			}

			hoisted.Add (parameter);
		}
	}


	class ExpressionInterpreter : ExpressionVisitor {

		readonly Stack<LambdaInfo> lambdas = new Stack<LambdaInfo> ();
		readonly Stack<object> stack = new Stack<object> ();

		readonly Dictionary<LambdaExpression, List<ParameterExpression>> hoisted_map;
		readonly Dictionary<ParameterExpression, object> hoisted_values;

		void Push (object value)
		{
			stack.Push (value);
		}

		object Pop ()
		{
			return stack.Pop ();
		}

		public ExpressionInterpreter (LambdaExpression lambda)
		{
			hoisted_map = new HoistedVariableDetector ().Process (lambda);

			if (hoisted_map != null)
				hoisted_values = new Dictionary<ParameterExpression, object> ();
		}

		private void VisitCoalesce (BinaryExpression binary)
		{
			Visit (binary.Left);

			var left = Pop ();

			if (left == null) {
				Visit (binary.Right);
				return;
			}

			if (binary.Conversion == null) {
				Push (left);
				return;
			}

			Push (Invoke (binary.Conversion.Compile (this), new [] { left }));
		}

		void VisitAndAlso (BinaryExpression binary)
		{
			object left = null;
			object right = null;

			Visit (binary.Left);

			left = Pop ();

			if (left == null || ((bool) left)) {
				Visit (binary.Right);
				right = Pop ();
			}

			Push (Math.And (left, right));
		}

		void VisitUserDefinedAndAlso (BinaryExpression binary)
		{
			object left = null;
			object right = null;

			Visit (binary.Left);

			left = Pop ();

			if (InvokeFalseOperator (binary, left)) {
				Push (left);
				return;
			}

			Visit (binary.Right);
			right = Pop ();

			if (binary.IsLiftedToNull && right == null) {
				Push (null);
				return;
			}

			Push (InvokeMethod (binary.Method, null, new [] { left, right }));
		}

		static bool InvokeTrueOperator (BinaryExpression binary, object target)
		{
			return (bool) InvokeMethod (GetTrueOperator (binary), null, new [] { target });
		}

		static bool InvokeFalseOperator (BinaryExpression binary, object target)
		{
			return (bool) InvokeMethod (GetFalseOperator (binary), null, new [] { target });
		}

		static MethodInfo GetFalseOperator (BinaryExpression binary)
		{
			return Expression.GetFalseOperator (binary.Left.Type.GetNotNullableType ());
		}

		static MethodInfo GetTrueOperator (BinaryExpression binary)
		{
			return Expression.GetTrueOperator (binary.Left.Type.GetNotNullableType ());
		}

		void VisitOrElse (BinaryExpression binary)
		{
			object left = null;
			object right = null;

			Visit (binary.Left);
			left = Pop ();

			if (left == null || !((bool) left)) {
				Visit (binary.Right);
				right = Pop ();
			}

			Push (Math.Or (left, right));
		}

		void VisitUserDefinedOrElse (BinaryExpression binary)
		{
			object left = null;
			object right = null;

			Visit (binary.Left);
			left = Pop ();

			if (InvokeTrueOperator (binary, left)) {
				Push (left);
				return;
			}

			Visit (binary.Right);
			right = Pop ();

			if (binary.IsLiftedToNull && right == null) {
				Push (null);
				return;
			}

			Push (InvokeMethod (binary.Method, null, new [] { left, right }));
		}

		void VisitLogicalBinary (BinaryExpression binary)
		{
			Visit (binary.Left);
			Visit (binary.Right);

			var right = Pop ();
			var left = Pop ();

			Push (Math.Evaluate (left, right, binary.Type, binary.NodeType));
		}

		void VisitArithmeticBinary (BinaryExpression binary)
		{
			Visit (binary.Left);
			Visit (binary.Right);

			if (IsNullBinaryLifting (binary))
				return;

			var right = Pop ();
			var left = Pop ();

			switch (binary.NodeType) {
			case ExpressionType.RightShift:
				Push (Math.RightShift (left, Convert.ToInt32 (right), Type.GetTypeCode (binary.Type.GetNotNullableType ())));
				return;
			case ExpressionType.LeftShift:
				Push (Math.LeftShift (left, Convert.ToInt32 (right), Type.GetTypeCode (binary.Type.GetNotNullableType ())));
				return;
			default:
				Push (Math.Evaluate (left, right, binary.Type, binary.NodeType));
				break;
			}
		}

		bool IsNullRelationalBinaryLifting (BinaryExpression binary)
		{
			var right = Pop ();
			var left = Pop ();

			if (binary.IsLifted && (left == null || right == null)) {
				if (binary.IsLiftedToNull) {
					Push (null);
					return true;
				}

				switch (binary.NodeType) {
				case ExpressionType.Equal:
					Push (BinaryEqual (binary, left, right));
					break;
				case ExpressionType.NotEqual:
					Push (BinaryNotEqual (binary, left, right));
					break;
				default:
					Push (false);
					break;
				}

				return true;
			}

			Push (left);
			Push (right);

			return false;
		}

		void VisitRelationalBinary (BinaryExpression binary)
		{
			Visit (binary.Left);
			Visit (binary.Right);

			if (IsNullRelationalBinaryLifting (binary))
				return;

			var right = Pop ();
			var left = Pop ();

			switch (binary.NodeType) {
			case ExpressionType.Equal:
				Push (BinaryEqual (binary, left, right));
				return;
			case ExpressionType.NotEqual:
				Push (BinaryNotEqual (binary, left, right));
				return;
			case ExpressionType.LessThan:
				Push (Comparer<object>.Default.Compare (left, right) < 0);
				return;
			case ExpressionType.LessThanOrEqual:
				Push (Comparer<object>.Default.Compare (left, right) <= 0);
				return;
			case ExpressionType.GreaterThan:
				Push (Comparer<object>.Default.Compare (left, right) > 0);
				return;
			case ExpressionType.GreaterThanOrEqual:
				Push (Comparer<object>.Default.Compare (left, right) >= 0);
				return;
			}
		}

		void VisitLogicalShortCircuitBinary (BinaryExpression binary)
		{
			switch (binary.NodeType) {
			case ExpressionType.AndAlso:
				VisitAndAlso (binary);
				return;
			case ExpressionType.OrElse:
				VisitOrElse (binary);
				return;
			}
		}

		void VisitArrayIndex (BinaryExpression binary)
		{
			Visit (binary.Left);
			var left = Pop ();
			Visit (binary.Right);
			var right = Pop ();

			Push (((Array) left).GetValue ((int) right));
		}

		bool IsNullBinaryLifting (BinaryExpression binary)
		{
			var right = Pop ();
			var left = Pop ();

			if (binary.IsLifted && (right == null || left == null)) {
				if (binary.IsLiftedToNull)
					Push (null);
				else
					Push (GetDefaultValue (binary.Type));

				return true;
			}

			Push (left);
			Push (right);

			return false;
		}

		static object GetDefaultValue (Type type)
		{
			var array = (Array) Array.CreateInstance (type, 1);
			return array.GetValue (0);
		}

		void VisitUserDefinedBinary (BinaryExpression binary)
		{
			switch (binary.NodeType) {
			case ExpressionType.AndAlso:
			case ExpressionType.OrElse:
				VisitUserDefinedLogicalShortCircuitBinary (binary);
				return;
			case ExpressionType.Equal:
			case ExpressionType.NotEqual:
				VisitUserDefinedRelationalBinary (binary);
				return;
			default:
				VisitUserDefinedCommonBinary (binary);
				return;
			}
		}

		void VisitUserDefinedLogicalShortCircuitBinary (BinaryExpression binary)
		{
			switch (binary.NodeType) {
			case ExpressionType.AndAlso:
				VisitUserDefinedAndAlso (binary);
				return;
			case ExpressionType.OrElse:
				VisitUserDefinedOrElse (binary);
				return;
			}
		}

		void VisitUserDefinedRelationalBinary (BinaryExpression binary)
		{
			Visit (binary.Left);
			Visit (binary.Right);

			if (IsNullRelationalBinaryLifting (binary))
				return;

			var right = Pop ();
			var left = Pop ();

			Push (InvokeBinary (binary, left, right));
		}

		void VisitUserDefinedCommonBinary (BinaryExpression binary)
		{
			Visit (binary.Left);
			Visit (binary.Right);

			if (IsNullBinaryLifting (binary))
				return;

			var right = Pop ();
			var left = Pop ();

			Push (InvokeBinary (binary, left, right));
		}

		object InvokeBinary (BinaryExpression binary, object left, object right)
		{
			return InvokeMethod (binary.Method, null, new [] { left, right });
		}

		bool BinaryEqual (BinaryExpression binary, object left, object right)
		{
			if (typeof (ValueType).IsAssignableFrom (binary.Right.Type))
				return ValueType.Equals (left, right);
			else
				return left == right;
		}

		bool BinaryNotEqual (BinaryExpression binary, object left, object right)
		{
			if (typeof (ValueType).IsAssignableFrom (binary.Right.Type))
				return !ValueType.Equals (left, right);
			else
				return left != right;
		}

		protected override void VisitBinary (BinaryExpression binary)
		{
			if (binary.Method != null) {
				VisitUserDefinedBinary (binary);
				return;
			}

			switch (binary.NodeType) {
			case ExpressionType.ArrayIndex:
				VisitArrayIndex (binary);
				return;
			case ExpressionType.Coalesce:
				VisitCoalesce (binary);
				return;
			case ExpressionType.AndAlso:
			case ExpressionType.OrElse:
				VisitLogicalShortCircuitBinary (binary);
				return;
			case ExpressionType.Equal:
			case ExpressionType.NotEqual:
			case ExpressionType.GreaterThan:
			case ExpressionType.GreaterThanOrEqual:
			case ExpressionType.LessThan:
			case ExpressionType.LessThanOrEqual:
				VisitRelationalBinary (binary);
				return;
			case ExpressionType.And:
			case ExpressionType.Or:
				VisitLogicalBinary (binary);
				return;
			case ExpressionType.Power:
			case ExpressionType.Add:
			case ExpressionType.AddChecked:
			case ExpressionType.Divide:
			case ExpressionType.ExclusiveOr:
			case ExpressionType.LeftShift:
			case ExpressionType.Modulo:
			case ExpressionType.Multiply:
			case ExpressionType.MultiplyChecked:
			case ExpressionType.RightShift:
			case ExpressionType.Subtract:
			case ExpressionType.SubtractChecked:
				VisitArithmeticBinary (binary);
				return;
			}
		}

		void VisitTypeAs (UnaryExpression unary)
		{
			Visit (unary.Operand);

			var value = Pop ();
			if (value == null || !Math.IsType (unary.Type, value))
				Push (null);
			else
				Push (value);
		}

		void VisitArrayLength (UnaryExpression unary)
		{
			Visit (unary.Operand);

			var array = (Array) Pop ();
			Push (array.Length);
		}

		void VisitConvert (UnaryExpression unary)
		{
			if (unary.NodeType == ExpressionType.ConvertChecked)
				VisitConvertChecked (unary);
			else
				VisitConvertUnchecked (unary);
		}

		void VisitConvertChecked (UnaryExpression unary)
		{
			VisitConvert (unary, Math.ConvertToTypeChecked);
		}

		void VisitConvertUnchecked (UnaryExpression unary)
		{
			VisitConvert (unary, Math.ConvertToTypeUnchecked);
		}

		void VisitConvert (UnaryExpression unary, Func<object, Type, Type, object> converter)
		{
			Visit (unary.Operand);
			Push (converter (Pop (), unary.Operand.Type, unary.Type));
		}

		bool IsNullUnaryLifting (UnaryExpression unary)
		{
			var value = Pop ();

			if (unary.IsLifted && value == null) {
				if (unary.IsLiftedToNull) {
					Push (null);
					return true;
				} else {
					throw new InvalidOperationException ();
				}
			}

			Push (value);
			return false;
		}

		void VisitQuote (UnaryExpression unary)
		{
			Push (unary.Operand);
		}

		void VisitUserDefinedUnary (UnaryExpression unary)
		{
			Visit (unary.Operand);

			if (IsNullUnaryLifting (unary))
				return;

			var value = Pop ();

			Push (InvokeUnary (unary, value));
		}

		object InvokeUnary (UnaryExpression unary, object value)
		{
			return InvokeMethod (unary.Method, null, new [] { value });
		}

		void VisitArithmeticUnary (UnaryExpression unary)
		{
			Visit (unary.Operand);

			if (IsNullUnaryLifting (unary))
				return;

			var value = Pop ();

			switch (unary.NodeType) {
			case ExpressionType.Not:
				if (unary.Type.GetNotNullableType () == typeof (bool))
					Push (!Convert.ToBoolean (value));
				else
					Push (~Convert.ToInt32 (value));
				return;
			case ExpressionType.Negate:
				Push (Math.Negate (value, Type.GetTypeCode (unary.Type.GetNotNullableType ())));
				return;
			case ExpressionType.NegateChecked:
				Push (Math.NegateChecked (value, Type.GetTypeCode (unary.Type.GetNotNullableType ())));
				return;
			case ExpressionType.UnaryPlus:
				Push (value);
				return;
			}
		}

		protected override void VisitUnary (UnaryExpression unary)
		{
			if (unary.Method != null) {
				VisitUserDefinedUnary (unary);
				return;
			}

			switch (unary.NodeType) {
			case ExpressionType.Quote:
				VisitQuote (unary);
				return;
			case ExpressionType.TypeAs:
				VisitTypeAs (unary);
				return;
			case ExpressionType.ArrayLength:
				VisitArrayLength (unary);
				return;
			case ExpressionType.Convert:
			case ExpressionType.ConvertChecked:
				VisitConvert (unary);
				return;
			case ExpressionType.Negate:
			case ExpressionType.NegateChecked:
			case ExpressionType.Not:
			case ExpressionType.UnaryPlus:
				VisitArithmeticUnary (unary);
				return;
			default:
				throw new NotImplementedException (unary.NodeType.ToString ());
			}
		}

		protected override void VisitNew (NewExpression nex)
		{
			if (nex.Constructor == null)
				Push (Activator.CreateInstance (nex.Type));
			else
				Push (InvokeConstructor (nex.Constructor, VisitListExpressions (nex.Arguments)));
		}

		static object InvokeConstructor (ConstructorInfo constructor, object [] arguments)
		{
			try {
				return constructor.Invoke (arguments);
			} catch (TargetInvocationException e) {
				throw e.InnerException;
			}
		}

		protected override void VisitTypeIs (TypeBinaryExpression type)
		{
			Visit (type.Expression);
			Push (Math.IsType (type.TypeOperand, Pop ()));
		}

		void VisitMemberInfo (MemberInfo mi)
		{
			mi.OnFieldOrProperty (
				field => {
					object target = null;
					if (!field.IsStatic)
						target = Pop ();

					Push (field.GetValue (target));
				},
				property => {
					object target = null;
					var getter = property.GetGetMethod (true);
					if (!getter.IsStatic)
						target = Pop ();

					Push (property.GetValue (target, null));
				});
		}

		protected override void VisitMemberAccess (MemberExpression member)
		{
			Visit (member.Expression);
			VisitMemberInfo (member.Member);
		}

		protected override void VisitNewArray (NewArrayExpression newArray)
		{
			switch (newArray.NodeType) {
			case ExpressionType.NewArrayInit:
				VisitNewArrayInit (newArray);
				return;
			case ExpressionType.NewArrayBounds:
				VisitNewArrayBounds (newArray);
				return;
			}

			throw new NotSupportedException ();
		}

		void VisitNewArrayBounds (NewArrayExpression newArray)
		{
			var lengths = new int [newArray.Expressions.Count];
			for (int i = 0; i < lengths.Length; i++) {
				Visit (newArray.Expressions [i]);
				lengths [i] = (int) Pop ();
			}

			Push (Array.CreateInstance (newArray.Type.GetElementType (), lengths));
		}

		void VisitNewArrayInit (NewArrayExpression newArray)
		{
			var array = Array.CreateInstance (
				newArray.Type.GetElementType (),
				newArray.Expressions.Count);

			for (int i = 0; i < array.Length; i++) {
				Visit (newArray.Expressions [i]);
				array.SetValue (Pop (), i);
			}

			Push (array);
		}

		protected override void VisitConditional (ConditionalExpression conditional)
		{
			Visit (conditional.Test);

			if ((bool) Pop ())
				Visit (conditional.IfTrue);
			else
				Visit (conditional.IfFalse);
		}

		protected override void VisitMethodCall (MethodCallExpression call)
		{
			object instance = null;
			if (call.Object != null) {
				Visit (call.Object);
				instance = Pop ();
			}

			Push (InvokeMethod (call.Method, instance, VisitListExpressions (call.Arguments)));
		}

		protected override void VisitParameter (ParameterExpression parameter)
		{
			var info = lambdas.Peek ();

			var lambda = info.Lambda;
			var arguments = info.Arguments;

			var index = GetParameterIndex (lambda, parameter);
			if (index >= 0) {
				Push (arguments [index]);
				return;
			}

			object value;
			if (hoisted_values.TryGetValue (parameter, out value)) {
				Push (value);
				return;
			}

			throw new ArgumentException ();
		}

		protected override void VisitConstant (ConstantExpression constant)
		{
			Push (constant.Value);
		}

		protected override void VisitInvocation (InvocationExpression invocation)
		{
			Visit (invocation.Expression);
			Push (Invoke ((Delegate) Pop (), VisitListExpressions (invocation.Arguments)));
		}

		static object Invoke (Delegate dlg, object [] arguments)
		{
			return InvokeMethod (dlg.Method, dlg.Target, arguments);
		}

		static object InvokeMethod (MethodBase method, object obj, object [] arguments)
		{
			try {
				return method.Invoke (obj, arguments);
			} catch (TargetInvocationException e) {
				throw e.InnerException;
			}
		}

		protected override void VisitMemberListBinding (MemberListBinding binding)
		{
			var value = Pop ();
			Push (value);
			VisitMemberInfo (binding.Member);
			VisitElementInitializerList (binding.Initializers);
			Pop (); // pop the member
			Push (value); // push the original target
		}

		protected override void VisitElementInitializer (ElementInit initializer)
		{
			object target = null;
			if (!initializer.AddMethod.IsStatic)
				target = Pop ();

			var arguments = VisitListExpressions (initializer.Arguments);
			InvokeMethod (initializer.AddMethod, target, arguments);

			if (!initializer.AddMethod.IsStatic)
				Push (target);
		}

		protected override void VisitMemberMemberBinding (MemberMemberBinding binding)
		{
			var value = Pop ();
			Push (value);
			VisitMemberInfo (binding.Member);
			VisitBindingList (binding.Bindings);
			Pop ();
			Push (value);
		}

		protected override void VisitMemberAssignment (MemberAssignment assignment)
		{
			Visit (assignment.Expression);

			var value = Pop ();

			assignment.Member.OnFieldOrProperty (
				field => {
					object target = null;
					if (!field.IsStatic)
						target = Pop ();

					field.SetValue (target, value);

					if (!field.IsStatic)
						Push (target);
				},
				property => {
					object target = null;
					var getter = property.GetGetMethod (true);
					if (!getter.IsStatic)
						target = Pop ();

					property.SetValue (target, value, null);

					if (!getter.IsStatic)
						Push (target);
				});
		}

		protected override void VisitLambda (LambdaExpression lambda)
		{
			Push (lambda.Compile (this));
		}

		private object [] VisitListExpressions (ReadOnlyCollection<Expression> collection)
		{
			object [] results = new object [collection.Count];
			for (int i = 0; i < results.Length; i++) {
				Visit (collection [i]);
				results [i] = Pop ();
			}

			return results;
		}

		void StoreHoistedVariables (LambdaExpression lambda, object [] arguments)
		{
			if (hoisted_map == null)
				return;

			List<ParameterExpression> variables;
			if (!hoisted_map.TryGetValue (lambda, out variables))
				return;

			foreach (var variable in variables)
				StoreHoistedVariable (variable, lambda, arguments);
		}

		void StoreHoistedVariable (ParameterExpression variable, LambdaExpression lambda, object [] arguments)
		{
			var index = GetParameterIndex (lambda, variable);
			if (index < 0)
				return;

			hoisted_values [variable] = arguments [index];
		}

		static int GetParameterIndex (LambdaExpression lambda, ParameterExpression parameter)
		{
			return lambda.Parameters.IndexOf (parameter);
		}

		public object Interpret (LambdaExpression lambda, object [] arguments)
		{
			lambdas.Push (new LambdaInfo (lambda, arguments));

			StoreHoistedVariables (lambda, arguments);

			Visit (lambda.Body);

			lambdas.Pop ();

			if (lambda.GetReturnType () != typeof (void))
				return Pop ();

			return null;
		}
	}
}
