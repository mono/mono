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
// Authors:
//
//	Copyright (C) 2006 Jordi Mas i Hernandez <jordimash@gmail.com>
//

using System;
using System.CodeDom;
using System.Reflection;
using System.Collections.Generic;

namespace System.Workflow.Activities.Rules
{
	internal class RuleExpressionBinaryOperatorResolver
	{
		protected RuleExpressionBinaryOperatorResolver ()
		{

		}

		static internal bool Evaluate (RuleExecution execution, CodeExpression expression)
		{
			CodeBinaryOperatorExpression code = (CodeBinaryOperatorExpression) expression;
			//Console.WriteLine ("RuleExpressionBinaryOperatorResolver {0}", expression);
			//Console.WriteLine ("Right {0}->{1}", code.Right, ((System.CodeDom.CodePrimitiveExpression)code.Right).Value);
			//Console.WriteLine ("Left {0}", code.Left);

			object right = GetValue (execution, code.Right);
			object left = GetValue (execution, code.Left);
			Type l = left.GetType ();

			//System.CodeDom.CodeFieldReferenceExpression leftexp = (System.CodeDom.CodeFieldReferenceExpression) code.left;
			////Console.WriteLine ("Left Value {0}", leftexp.TargetObject);
			//Console.WriteLine ("Left Value {0}", leftexp.TargetObject.UserData);

			//Console.WriteLine ("Right Value {0}", right);
			//Console.WriteLine ("Left Value {0}", left);

			//Console.WriteLine ("Right Value type {0}", right.GetType ());
			//Console.WriteLine ("Left Value type {0}", left.GetType ());

			BinaryOperatorType bot = CreateInstanceForType (l, code, right, left);
			return bot.Evaluate ();
		}

		private static BinaryOperatorType CreateInstanceForType (Type l, CodeBinaryOperatorExpression code, object right, object left)
		{
			if (l == typeof (Int32))
				return new BinaryOperatorTypeInt32 (code, right, left);

			if (l == typeof (Int16))
				return new BinaryOperatorTypeInt16 (code, right, left);

			if (l == typeof (Int64))
				return new BinaryOperatorTypeInt64 (code, right, left);

			if (l == typeof (sbyte))
				return new BinaryOperatorTypeSbyte (code, right, left);

			if (l == typeof (float))
				return new BinaryOperatorTypeFloat (code, right, left);

			if (l == typeof (char))
				return new BinaryOperatorTypeChar (code, right, left);

			if (l == typeof (byte))
				return new BinaryOperatorTypeByte (code, right, left);

			throw new InvalidOperationException ("Type not suported as binary operator");
		}

		private static object GetValue (RuleExecution execution, CodeExpression obj)
		{
			if (obj.GetType () == typeof (System.CodeDom.CodePrimitiveExpression)) {
				return (((System.CodeDom.CodePrimitiveExpression) obj).Value);
			}

			if (obj.GetType () == typeof (CodeFieldReferenceExpression)) {
				return RuleExpressionCondition.CodeFieldReferenceValue (execution, obj);
			}

			if (obj.GetType () == typeof (CodePropertyReferenceExpression)) {
				return RuleExpressionCondition.CodePropertyReferenceValue (execution, obj);
			}

			return null;
		}
	}

	// class BinaryOperatorType
	internal class BinaryOperatorType
	{
		protected CodeBinaryOperatorExpression expression;
		protected Type r;
		protected object right;
		protected object left;

		public BinaryOperatorType (CodeBinaryOperatorExpression expression, object right, object left)
		{
			this.expression = expression;
			this.right = right;
			this.left = left;
			r = right.GetType ();
		}

		public bool TypeNotSupported ()
		{
			throw new InvalidOperationException ("Type not suported as binary operator");
		}

		public bool Evaluate ()
		{
			switch (expression.Operator) {
				case CodeBinaryOperatorType.Add:
					break;
				case CodeBinaryOperatorType.Assign:
					break;
				case CodeBinaryOperatorType.BitwiseAnd:
					break;
				case CodeBinaryOperatorType.BitwiseOr:
					break;
				case CodeBinaryOperatorType.BooleanAnd:
					break;
				case CodeBinaryOperatorType.BooleanOr:
					break;
				case CodeBinaryOperatorType.Divide:
					break;
				case CodeBinaryOperatorType.GreaterThan:
					return GreaterThan ();
				case CodeBinaryOperatorType.GreaterThanOrEqual:
					return GreaterThanOrEqual ();
				case CodeBinaryOperatorType.IdentityEquality:
					break;
				case CodeBinaryOperatorType.IdentityInequality:
					break;
				case CodeBinaryOperatorType.LessThan:
					return LessThan ();
				case CodeBinaryOperatorType.LessThanOrEqual:
					return LessThanOrEqual ();
				case CodeBinaryOperatorType.Modulus:
					break;
				case CodeBinaryOperatorType.Multiply:
					break;
				case CodeBinaryOperatorType.Subtract:
					break;
				case CodeBinaryOperatorType.ValueEquality:
					return ValueEquality ();
				default:
					break;
			}

			return false;
		}

		public virtual bool BitwiseAnd ()
		{
			return TypeNotSupported ();
		}

		public virtual bool LessThan ()
		{
			return TypeNotSupported ();
		}

		public virtual bool LessThanOrEqual ()
		{
			return TypeNotSupported ();
		}

		public virtual bool GreaterThan ()
		{
			return TypeNotSupported ();
		}

		public virtual bool GreaterThanOrEqual ()
		{
			return TypeNotSupported ();
		}

		public virtual bool ValueEquality ()
		{
			return TypeNotSupported ();
		}
	}

	// class BinaryOperatorTypeInt32
	internal class BinaryOperatorTypeInt32 : BinaryOperatorType
	{
		public BinaryOperatorTypeInt32 (CodeBinaryOperatorExpression expression, object right, object left)
			: base (expression, right, left) {}

		public override bool LessThan ()
		{
			if (r == typeof (Int32)) return (Int32) left < (Int32) right;
			if (r == typeof (Int16)) return (Int32) left < (Int16) right;
			if (r == typeof (Int64)) return (Int32) left < (Int64) right;
			if (r == typeof (sbyte)) return (Int32) left < (sbyte) right;
			if (r == typeof (float)) return (Int32) left < (float) right;
			if (r == typeof (char)) return (Int32) left < (char) right;
			if (r == typeof (byte)) return (Int32) left < (byte) right;

			return TypeNotSupported ();
		}

		public override bool LessThanOrEqual ()
		{
			if (r == typeof (Int32)) return (Int32) left <= (Int32) right;
			if (r == typeof (Int16)) return (Int32) left <= (Int16) right;
			if (r == typeof (Int64)) return (Int32) left <= (Int64) right;
			if (r == typeof (sbyte)) return (Int32) left <= (sbyte) right;
			if (r == typeof (float)) return (Int32) left <= (float) right;
			if (r == typeof (char)) return (Int32) left <= (char) right;
			if (r == typeof (byte)) return (Int32) left <= (byte) right;

			return TypeNotSupported ();
		}

		public override bool GreaterThan ()
		{
			if (r == typeof (Int32)) return (Int32) left > (Int32) right;
			if (r == typeof (Int16)) return (Int32) left > (Int16) right;
			if (r == typeof (Int64)) return (Int32) left > (Int64) right;
			if (r == typeof (sbyte)) return (Int32) left > (sbyte) right;
			if (r == typeof (float)) return (Int32) left > (float) right;
			if (r == typeof (char)) return (Int32) left > (char) right;
			if (r == typeof (byte)) return (Int32) left > (byte) right;

			return TypeNotSupported ();
		}

		public override bool GreaterThanOrEqual ()
		{
			if (r == typeof (Int32)) return (Int32) left >= (Int32) right;
			if (r == typeof (Int16)) return (Int32) left >= (Int16) right;
			if (r == typeof (Int64)) return (Int32) left >= (Int64) right;
			if (r == typeof (sbyte)) return (Int32) left >= (sbyte) right;
			if (r == typeof (float)) return (Int32) left >= (float) right;
			if (r == typeof (char)) return (Int32) left >= (char) right;
			if (r == typeof (byte)) return (Int32) left >= (byte) right;

			return TypeNotSupported ();
		}

		public override bool ValueEquality ()
		{
			if (r == typeof (Int32)) return (Int32) left == (Int32) right;
			if (r == typeof (Int16)) return (Int32) left == (Int16) right;
			if (r == typeof (Int64)) return (Int32) left == (Int64) right;
			if (r == typeof (sbyte)) return (Int32) left == (sbyte) right;
			if (r == typeof (float)) return (Int32) left == (float) right;
			if (r == typeof (char)) return (Int32) left == (char) right;
			if (r == typeof (byte)) return (Int32) left == (byte) right;

			return TypeNotSupported ();
		}
	}

	// class BinaryOperatorTypeInt16
	internal class BinaryOperatorTypeInt16 : BinaryOperatorType
	{
		public BinaryOperatorTypeInt16 (CodeBinaryOperatorExpression expression, object right, object left)
			: base (expression, right, left) {}

		public override bool LessThan ()
		{
			if (r == typeof (Int32)) return (Int16) left < (Int32) right;
			if (r == typeof (Int16)) return (Int16) left < (Int16) right;
			if (r == typeof (Int64)) return (Int16) left < (Int64) right;
			if (r == typeof (sbyte)) return (Int16) left < (sbyte) right;
			if (r == typeof (float)) return (Int16) left < (float) right;
			if (r == typeof (char)) return (Int16) left < (char) right;
			if (r == typeof (byte)) return (Int16) left < (byte) right;

			return TypeNotSupported ();
		}

		public override bool LessThanOrEqual ()
		{
			if (r == typeof (Int32)) return (Int16) left <= (Int32) right;
			if (r == typeof (Int16)) return (Int16) left <= (Int16) right;
			if (r == typeof (Int64)) return (Int16) left <= (Int64) right;
			if (r == typeof (sbyte)) return (Int16) left <= (sbyte) right;
			if (r == typeof (float)) return (Int16) left <= (float) right;
			if (r == typeof (char)) return (Int16) left <= (char) right;
			if (r == typeof (byte)) return (Int16) left <= (byte) right;

			return TypeNotSupported ();
		}

		public override bool GreaterThan ()
		{
			if (r == typeof (Int32)) return (Int16) left > (Int32) right;
			if (r == typeof (Int16)) return (Int16) left > (Int16) right;
			if (r == typeof (Int64)) return (Int16) left > (Int64) right;
			if (r == typeof (sbyte)) return (Int16) left > (sbyte) right;
			if (r == typeof (float)) return (Int16) left > (float) right;
			if (r == typeof (char)) return (Int16) left > (char) right;
			if (r == typeof (byte)) return (Int16) left > (byte) right;

			return TypeNotSupported ();
		}

		public override bool GreaterThanOrEqual ()
		{
			if (r == typeof (Int32)) return (Int16) left >= (Int32) right;
			if (r == typeof (Int16)) return (Int16) left >= (Int16) right;
			if (r == typeof (Int64)) return (Int16) left >= (Int64) right;
			if (r == typeof (sbyte)) return (Int16) left >= (sbyte) right;
			if (r == typeof (float)) return (Int16) left >= (float) right;
			if (r == typeof (char)) return (Int16) left >= (char) right;
			if (r == typeof (byte)) return (Int16) left >= (byte) right;

			return TypeNotSupported ();
		}

		public override bool ValueEquality ()
		{
			if (r == typeof (Int32)) return (Int16) left == (Int32) right;
			if (r == typeof (Int16)) return (Int16) left == (Int16) right;
			if (r == typeof (Int64)) return (Int16) left == (Int64) right;
			if (r == typeof (sbyte)) return (Int16) left == (sbyte) right;
			if (r == typeof (float)) return (Int16) left == (float) right;
			if (r == typeof (char)) return (Int16) left == (char) right;
			if (r == typeof (byte)) return (Int16) left == (byte) right;

			return TypeNotSupported ();
		}
	}

	// class BinaryOperatorTypeInt64
	internal class BinaryOperatorTypeInt64 : BinaryOperatorType
	{
		public BinaryOperatorTypeInt64 (CodeBinaryOperatorExpression expression, object right, object left)
			: base (expression, right, left) {}

		public override bool LessThan ()
		{
			if (r == typeof (Int32)) return (Int64) left < (Int32) right;
			if (r == typeof (Int16)) return (Int64) left < (Int16) right;
			if (r == typeof (Int64)) return (Int64) left < (Int64) right;
			if (r == typeof (sbyte)) return (Int64) left < (sbyte) right;
			if (r == typeof (float)) return (Int64) left < (float) right;
			if (r == typeof (char)) return (Int64) left < (char) right;
			if (r == typeof (byte)) return (Int64) left < (byte) right;

			return TypeNotSupported ();
		}

		public override bool LessThanOrEqual ()
		{
			if (r == typeof (Int32)) return (Int64) left <= (Int32) right;
			if (r == typeof (Int16)) return (Int64) left <= (Int16) right;
			if (r == typeof (Int64)) return (Int64) left <= (Int64) right;
			if (r == typeof (sbyte)) return (Int64) left <= (sbyte) right;
			if (r == typeof (float)) return (Int64) left <= (float) right;
			if (r == typeof (char)) return (Int64) left <= (char) right;
			if (r == typeof (byte)) return (Int64) left <= (byte) right;

			return TypeNotSupported ();
		}

		public override bool GreaterThan ()
		{
			if (r == typeof (Int32)) return (Int64) left > (Int32) right;
			if (r == typeof (Int16)) return (Int64) left > (Int16) right;
			if (r == typeof (Int64)) return (Int64) left > (Int64) right;
			if (r == typeof (sbyte)) return (Int64) left > (sbyte) right;
			if (r == typeof (float)) return (Int64) left > (float) right;
			if (r == typeof (char)) return (Int64) left > (char) right;
			if (r == typeof (byte)) return (Int64) left > (byte) right;

			return TypeNotSupported ();
		}

		public override bool GreaterThanOrEqual ()
		{
			if (r == typeof (Int32)) return (Int64) left >= (Int32) right;
			if (r == typeof (Int16)) return (Int64) left >= (Int16) right;
			if (r == typeof (Int64)) return (Int64) left >= (Int64) right;
			if (r == typeof (sbyte)) return (Int64) left >= (sbyte) right;
			if (r == typeof (float)) return (Int64) left >= (float) right;
			if (r == typeof (char)) return (Int64) left >= (char) right;
			if (r == typeof (byte)) return (Int64) left >= (byte) right;

			return TypeNotSupported ();
		}

		public override bool ValueEquality ()
		{
			if (r == typeof (Int32)) return (Int64) left == (Int32) right;
			if (r == typeof (Int16)) return (Int64) left == (Int16) right;
			if (r == typeof (Int64)) return (Int64) left == (Int64) right;
			if (r == typeof (sbyte)) return (Int64) left == (sbyte) right;
			if (r == typeof (float)) return (Int64) left == (float) right;
			if (r == typeof (char)) return (Int64) left == (char) right;
			if (r == typeof (byte)) return (Int64) left == (byte) right;

			return TypeNotSupported ();
		}
	}

	// class BinaryOperatorTypeSbyte
	internal class BinaryOperatorTypeSbyte : BinaryOperatorType
	{
		public BinaryOperatorTypeSbyte (CodeBinaryOperatorExpression expression, object right, object left)
			: base (expression, right, left) {}

		public override bool LessThan ()
		{
			if (r == typeof (Int32)) return (sbyte) left < (Int32) right;
			if (r == typeof (Int16)) return (sbyte) left < (Int16) right;
			if (r == typeof (Int64)) return (sbyte) left < (Int64) right;
			if (r == typeof (sbyte)) return (sbyte) left < (sbyte) right;
			if (r == typeof (float)) return (sbyte) left < (float) right;
			if (r == typeof (char)) return (sbyte) left < (char) right;
			if (r == typeof (byte)) return (sbyte) left < (byte) right;

			return TypeNotSupported ();
		}

		public override bool LessThanOrEqual ()
		{
			if (r == typeof (Int32)) return (sbyte) left <= (Int32) right;
			if (r == typeof (Int16)) return (sbyte) left <= (Int16) right;
			if (r == typeof (Int64)) return (sbyte) left <= (Int64) right;
			if (r == typeof (sbyte)) return (sbyte) left <= (sbyte) right;
			if (r == typeof (float)) return (sbyte) left <= (float) right;
			if (r == typeof (char)) return (sbyte) left <= (char) right;
			if (r == typeof (byte)) return (sbyte) left <= (byte) right;

			return TypeNotSupported ();
		}

		public override bool GreaterThan ()
		{
			if (r == typeof (Int32)) return (sbyte) left > (Int32) right;
			if (r == typeof (Int16)) return (sbyte) left > (Int16) right;
			if (r == typeof (Int64)) return (sbyte) left > (Int64) right;
			if (r == typeof (sbyte)) return (sbyte) left > (sbyte) right;
			if (r == typeof (float)) return (sbyte) left > (float) right;
			if (r == typeof (char)) return (sbyte) left > (char) right;
			if (r == typeof (byte)) return (sbyte) left > (byte) right;

			return TypeNotSupported ();
		}

		public override bool GreaterThanOrEqual ()
		{
			if (r == typeof (Int32)) return (sbyte) left >= (Int32) right;
			if (r == typeof (Int16)) return (sbyte) left >= (Int16) right;
			if (r == typeof (Int64)) return (sbyte) left >= (Int64) right;
			if (r == typeof (sbyte)) return (sbyte) left >= (sbyte) right;
			if (r == typeof (float)) return (sbyte) left >= (float) right;
			if (r == typeof (char)) return (sbyte) left >= (char) right;
			if (r == typeof (byte)) return (sbyte) left >= (byte) right;

			return TypeNotSupported ();
		}

		public override bool ValueEquality ()
		{
			if (r == typeof (Int32)) return (sbyte) left == (Int32) right;
			if (r == typeof (Int16)) return (sbyte) left == (Int16) right;
			if (r == typeof (Int64)) return (sbyte) left == (Int64) right;
			if (r == typeof (sbyte)) return (sbyte) left == (sbyte) right;
			if (r == typeof (float)) return (sbyte) left == (float) right;
			if (r == typeof (char)) return (sbyte) left == (char) right;
			if (r == typeof (byte)) return (sbyte) left == (byte) right;

			return TypeNotSupported ();
		}
	}

	// class BinaryOperatorTypeFloat
	internal class BinaryOperatorTypeFloat : BinaryOperatorType
	{
		public BinaryOperatorTypeFloat (CodeBinaryOperatorExpression expression, object right, object left)
			: base (expression, right, left) {}

		public override bool LessThan ()
		{
			if (r == typeof (Int32)) return (float) left < (Int32) right;
			if (r == typeof (Int16)) return (float) left < (Int16) right;
			if (r == typeof (Int64)) return (float) left < (Int64) right;
			if (r == typeof (sbyte)) return (float) left < (sbyte) right;
			if (r == typeof (float)) return (float) left < (float) right;
			if (r == typeof (char)) return (float) left < (char) right;
			if (r == typeof (byte)) return (float) left < (byte) right;

			return TypeNotSupported ();
		}

		public override bool LessThanOrEqual ()
		{
			if (r == typeof (Int32)) return (float) left <= (Int32) right;
			if (r == typeof (Int16)) return (float) left <= (Int16) right;
			if (r == typeof (Int64)) return (float) left <= (Int64) right;
			if (r == typeof (sbyte)) return (float) left <= (sbyte) right;
			if (r == typeof (float)) return (float) left <= (float) right;
			if (r == typeof (char)) return (float) left <= (char) right;
			if (r == typeof (byte)) return (float) left <= (byte) right;

			return TypeNotSupported ();
		}

		public override bool GreaterThan ()
		{
			if (r == typeof (Int32)) return (float) left > (Int32) right;
			if (r == typeof (Int16)) return (float) left > (Int16) right;
			if (r == typeof (Int64)) return (float) left > (Int64) right;
			if (r == typeof (sbyte)) return (float) left > (sbyte) right;
			if (r == typeof (float)) return (float) left > (float) right;
			if (r == typeof (char)) return (float) left > (char) right;
			if (r == typeof (byte)) return (float) left > (byte) right;

			return TypeNotSupported ();
		}

		public override bool GreaterThanOrEqual ()
		{
			if (r == typeof (Int32)) return (float) left >= (Int32) right;
			if (r == typeof (Int16)) return (float) left >= (Int16) right;
			if (r == typeof (Int64)) return (float) left >= (Int64) right;
			if (r == typeof (sbyte)) return (float) left >= (sbyte) right;
			if (r == typeof (float)) return (float) left >= (float) right;
			if (r == typeof (char)) return (float) left >= (char) right;
			if (r == typeof (byte)) return (float) left >= (byte) right;

			return TypeNotSupported ();
		}

		public override bool ValueEquality ()
		{
			if (r == typeof (Int32)) return (float) left == (Int32) right;
			if (r == typeof (Int16)) return (float) left == (Int16) right;
			if (r == typeof (Int64)) return (float) left == (Int64) right;
			if (r == typeof (sbyte)) return (float) left == (sbyte) right;
			if (r == typeof (float)) return (float) left == (float) right;
			if (r == typeof (char)) return (float) left == (char) right;
			if (r == typeof (byte)) return (float) left == (byte) right;

			return TypeNotSupported ();
		}
	}

	// class BinaryOperatorTypeChar
	internal class BinaryOperatorTypeChar : BinaryOperatorType
	{
		public BinaryOperatorTypeChar (CodeBinaryOperatorExpression expression, object right, object left)
			: base (expression, right, left) {}

		public override bool LessThan ()
		{
			if (r == typeof (Int32)) return (char) left < (Int32) right;
			if (r == typeof (Int16)) return (char) left < (Int16) right;
			if (r == typeof (Int64)) return (char) left < (Int64) right;
			if (r == typeof (sbyte)) return (char) left < (sbyte) right;
			if (r == typeof (float)) return (char) left < (float) right;
			if (r == typeof (char)) return (char) left < (char) right;
			if (r == typeof (byte)) return (char) left < (byte) right;

			return TypeNotSupported ();
		}

		public override bool LessThanOrEqual ()
		{
			if (r == typeof (Int32)) return (char) left <= (Int32) right;
			if (r == typeof (Int16)) return (char) left <= (Int16) right;
			if (r == typeof (Int64)) return (char) left <= (Int64) right;
			if (r == typeof (sbyte)) return (char) left <= (sbyte) right;
			if (r == typeof (float)) return (char) left <= (float) right;
			if (r == typeof (char)) return (char) left <= (char) right;
			if (r == typeof (byte)) return (char) left <= (byte) right;

			return TypeNotSupported ();
		}

		public override bool GreaterThan ()
		{
			if (r == typeof (Int32)) return (char) left > (Int32) right;
			if (r == typeof (Int16)) return (char) left > (Int16) right;
			if (r == typeof (Int64)) return (char) left > (Int64) right;
			if (r == typeof (sbyte)) return (char) left > (sbyte) right;
			if (r == typeof (float)) return (char) left > (float) right;
			if (r == typeof (char)) return (char) left > (char) right;
			if (r == typeof (byte)) return (char) left > (byte) right;

			return TypeNotSupported ();
		}

		public override bool GreaterThanOrEqual ()
		{
			if (r == typeof (Int32)) return (char) left >= (Int32) right;
			if (r == typeof (Int16)) return (char) left >= (Int16) right;
			if (r == typeof (Int64)) return (char) left >= (Int64) right;
			if (r == typeof (sbyte)) return (char) left >= (sbyte) right;
			if (r == typeof (float)) return (char) left >= (float) right;
			if (r == typeof (char)) return (char) left >= (char) right;
			if (r == typeof (byte)) return (char) left >= (byte) right;

			return TypeNotSupported ();
		}

		public override bool ValueEquality ()
		{
			if (r == typeof (Int32)) return (char) left == (Int32) right;
			if (r == typeof (Int16)) return (char) left == (Int16) right;
			if (r == typeof (Int64)) return (char) left == (Int64) right;
			if (r == typeof (sbyte)) return (char) left == (sbyte) right;
			if (r == typeof (float)) return (char) left == (float) right;
			if (r == typeof (char)) return (char) left == (char) right;
			if (r == typeof (byte)) return (char) left == (byte) right;

			return TypeNotSupported ();
		}
	}

	// class BinaryOperatorTypeByte
	internal class BinaryOperatorTypeByte : BinaryOperatorType
	{
		public BinaryOperatorTypeByte (CodeBinaryOperatorExpression expression, object right, object left)
			: base (expression, right, left) {}

		public override bool LessThan ()
		{
			if (r == typeof (Int32)) return (byte) left < (Int32) right;
			if (r == typeof (Int16)) return (byte) left < (Int16) right;
			if (r == typeof (Int64)) return (byte) left < (Int64) right;
			if (r == typeof (sbyte)) return (byte) left < (sbyte) right;
			if (r == typeof (float)) return (byte) left < (float) right;
			if (r == typeof (char)) return (byte) left < (char) right;
			if (r == typeof (byte)) return (byte) left < (byte) right;

			return TypeNotSupported ();
		}

		public override bool LessThanOrEqual ()
		{
			if (r == typeof (Int32)) return (byte) left <= (Int32) right;
			if (r == typeof (Int16)) return (byte) left <= (Int16) right;
			if (r == typeof (Int64)) return (byte) left <= (Int64) right;
			if (r == typeof (sbyte)) return (byte) left <= (sbyte) right;
			if (r == typeof (float)) return (byte) left <= (float) right;
			if (r == typeof (char)) return (byte) left <= (char) right;
			if (r == typeof (byte)) return (byte) left <= (byte) right;

			return TypeNotSupported ();
		}

		public override bool GreaterThan ()
		{
			if (r == typeof (Int32)) return (byte) left > (Int32) right;
			if (r == typeof (Int16)) return (byte) left > (Int16) right;
			if (r == typeof (Int64)) return (byte) left > (Int64) right;
			if (r == typeof (sbyte)) return (byte) left > (sbyte) right;
			if (r == typeof (float)) return (byte) left > (float) right;
			if (r == typeof (char)) return (byte) left > (char) right;
			if (r == typeof (byte)) return (byte) left > (byte) right;

			return TypeNotSupported ();
		}

		public override bool GreaterThanOrEqual ()
		{
			if (r == typeof (Int32)) return (byte) left >= (Int32) right;
			if (r == typeof (Int16)) return (byte) left >= (Int16) right;
			if (r == typeof (Int64)) return (byte) left >= (Int64) right;
			if (r == typeof (sbyte)) return (byte) left >= (sbyte) right;
			if (r == typeof (float)) return (byte) left >= (float) right;
			if (r == typeof (char)) return (byte) left >= (char) right;
			if (r == typeof (byte)) return (byte) left >= (byte) right;

			return TypeNotSupported ();
		}

		public override bool ValueEquality ()
		{
			if (r == typeof (Int32)) return (byte) left == (Int32) right;
			if (r == typeof (Int16)) return (byte) left == (Int16) right;
			if (r == typeof (Int64)) return (byte) left == (Int64) right;
			if (r == typeof (sbyte)) return (byte) left == (sbyte) right;
			if (r == typeof (float)) return (byte) left == (float) right;
			if (r == typeof (char)) return (byte) left == (char) right;
			if (r == typeof (byte)) return (byte) left == (byte) right;

			return TypeNotSupported ();
		}
	}
}

