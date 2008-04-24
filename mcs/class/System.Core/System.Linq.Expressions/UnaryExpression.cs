//
// UnaryExpression.cs
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
using System.Reflection;
using System.Reflection.Emit;

namespace System.Linq.Expressions {

	public sealed class UnaryExpression : Expression {

		Expression operand;
		MethodInfo method;
		bool is_lifted;

		public Expression Operand {
			get { return operand; }
		}

		public MethodInfo Method {
			get { return method; }
		}

		public bool IsLifted {
			get { return is_lifted; }
		}

		public bool IsLiftedToNull {
			get { return is_lifted && IsNullable (this.Type); }
		}

		internal UnaryExpression (ExpressionType node_type, Expression operand, Type type)
			: base (node_type, type)
		{
			this.operand = operand;
		}

		internal UnaryExpression (ExpressionType node_type, Expression operand, Type type, MethodInfo method, bool is_lifted)
			: base (node_type, type)
		{
			this.operand = operand;
			this.method = method;
			this.is_lifted = is_lifted;
		}

		void EmitArrayLength (EmitContext ec)
		{
			operand.Emit (ec);
			ec.ig.Emit (OpCodes.Ldlen);
		}

		void EmitTypeAs (EmitContext ec)
		{
			var type = this.Type;

			ec.EmitIsInst (operand, type);

			if (IsNullable (type))
				ec.ig.Emit (OpCodes.Unbox_Any, type);
		}

		void EmitUnaryOperator (EmitContext ec)
		{
			var ig = ec.ig;

			switch (NodeType) {
			case ExpressionType.Not:
				if (GetNotNullableOf (operand.Type) == typeof (bool)) {
					ig.Emit (OpCodes.Ldc_I4_0);
					ig.Emit (OpCodes.Ceq);
				} else
					ig.Emit (OpCodes.Not);
				break;
			case ExpressionType.Negate:
			case ExpressionType.NegateChecked:
				ig.Emit (OpCodes.Neg);
				break;
			case ExpressionType.Convert:
			case ExpressionType.ConvertChecked:
				EmitConvert (ec);
				break;
			}
		}

		void EmitConvert (EmitContext ec)
		{
			var from = operand.Type;
			var target = Type;

			if (from == target)
				return;

			if (IsReferenceConversion (from, target))
				EmitCast (ec);
			else if (IsPrimitiveConversion (from, target))
				EmitPrimitiveConversion (ec);
			else
				throw new NotImplementedException ();
		}

		bool IsBoxing ()
		{
			return operand.Type.IsValueType && !Type.IsValueType;
		}

		bool IsUnBoxing ()
		{
			return !operand.Type.IsValueType && Type.IsValueType;
		}

		void EmitCast (EmitContext ec)
		{
			var ig = ec.ig;

			if (IsBoxing ()) {
				ig.Emit (OpCodes.Box, operand.Type);
			} else if (IsUnBoxing ()) {
				ig.Emit (OpCodes.Unbox_Any, Type);
			} else
				ec.ig.Emit (OpCodes.Castclass, Type);
		}

		void EmitPrimitiveConversion (EmitContext ec)
		{
			throw new NotImplementedException ();
		}

		internal override void Emit (EmitContext ec)
		{
			switch (this.NodeType) {
			case ExpressionType.ArrayLength:
				EmitArrayLength (ec);
				return;
			case ExpressionType.TypeAs:
				EmitTypeAs (ec);
				return;
			case ExpressionType.Convert:
			case ExpressionType.ConvertChecked:
			case ExpressionType.Not:
			case ExpressionType.Negate:
			case ExpressionType.NegateChecked:
			case ExpressionType.UnaryPlus:
				if (!is_lifted) {
					operand.Emit (ec);
					EmitUnaryOperator (ec);
				} else
					throw new NotImplementedException ();
				return;
			case ExpressionType.Quote:
				ec.EmitReadGlobal (operand, typeof (Expression));
				return;
			default:
				throw new NotImplementedException (this.NodeType.ToString ());
			}
		}
	}
}
