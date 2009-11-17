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
			get { return is_lifted && this.Type.IsNullable (); }
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

			if (type.IsNullable ())
				ec.ig.Emit (OpCodes.Unbox_Any, type);
		}

		void EmitLiftedUnary (EmitContext ec)
		{
			var ig = ec.ig;

			var from = ec.EmitStored (operand);
			var to = ig.DeclareLocal (Type);

			var has_value = ig.DefineLabel ();
			var done = ig.DefineLabel ();

			ec.EmitNullableHasValue (from);
			ig.Emit (OpCodes.Brtrue, has_value);

			// if not has value
			ec.EmitNullableInitialize (to);

			ig.Emit (OpCodes.Br, done);

			ig.MarkLabel (has_value);
			// if has value
			ec.EmitNullableGetValueOrDefault (from);

			EmitUnaryOperator (ec);

			ec.EmitNullableNew (Type);

			ig.MarkLabel (done);
		}

		void EmitUnaryOperator (EmitContext ec)
		{
			var ig = ec.ig;

			switch (NodeType) {
			case ExpressionType.Not:
				if (operand.Type.GetNotNullableType () == typeof (bool)) {
					ig.Emit (OpCodes.Ldc_I4_0);
					ig.Emit (OpCodes.Ceq);
				} else
					ig.Emit (OpCodes.Not);
				break;
			case ExpressionType.Negate:
				ig.Emit (OpCodes.Neg);
				break;
			case ExpressionType.NegateChecked:
				ig.Emit (OpCodes.Ldc_I4_M1);
				ig.Emit (IsUnsigned (operand.Type) ? OpCodes.Mul_Ovf_Un : OpCodes.Mul_Ovf);
				break;
			case ExpressionType.Convert:
			case ExpressionType.ConvertChecked:
				// Called when converting from nullable from nullable
				EmitPrimitiveConversion (ec,
					operand.Type.GetNotNullableType (),
					Type.GetNotNullableType ());
				break;
			}
		}

		void EmitConvert (EmitContext ec)
		{
			var from = operand.Type;
			var target = Type;

			if (from == target)
				operand.Emit (ec);
			else if (from.IsNullable () && !target.IsNullable ())
				EmitConvertFromNullable (ec);
			else if (!from.IsNullable () && target.IsNullable ())
				EmitConvertToNullable (ec);
			else if (from.IsNullable () && target.IsNullable ())
				EmitConvertFromNullableToNullable (ec);
			else if (IsReferenceConversion (from, target))
				EmitCast (ec);
			else if (IsPrimitiveConversion (from, target))
				EmitPrimitiveConversion (ec);
			else
				throw new NotImplementedException ();
		}

		void EmitConvertFromNullableToNullable (EmitContext ec)
		{
			EmitLiftedUnary (ec);
		}

		void EmitConvertToNullable (EmitContext ec)
		{
			ec.Emit (operand);

			if (IsUnBoxing ()) {
				EmitUnbox (ec);
				return;
			}

			if (operand.Type != Type.GetNotNullableType ()) {
				EmitPrimitiveConversion (ec,
					operand.Type,
					Type.GetNotNullableType ());
			}

			ec.EmitNullableNew (Type);
		}

		void EmitConvertFromNullable (EmitContext ec)
		{
			if (IsBoxing ()) {
				ec.Emit (operand);
				EmitBox (ec);
				return;
			}

			ec.EmitCall (operand, operand.Type.GetMethod ("get_Value"));

			if (operand.Type.GetNotNullableType () != Type) {
				EmitPrimitiveConversion (ec,
					operand.Type.GetNotNullableType (),
					Type);
			}
		}

		bool IsBoxing ()
		{
			return operand.Type.IsValueType && !Type.IsValueType;
		}

		void EmitBox (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Box, operand.Type);
		}

		bool IsUnBoxing ()
		{
			return !operand.Type.IsValueType && Type.IsValueType;
		}

		void EmitUnbox (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Unbox_Any, Type);
		}

		void EmitCast (EmitContext ec)
		{
			operand.Emit (ec);

			if (IsBoxing ()) {
				EmitBox (ec);
			} else if (IsUnBoxing ()) {
				EmitUnbox (ec);
			} else
				ec.ig.Emit (OpCodes.Castclass, Type);
		}

		void EmitPrimitiveConversion (EmitContext ec, bool is_unsigned,
			OpCode signed, OpCode unsigned, OpCode signed_checked, OpCode unsigned_checked)
		{
			if (this.NodeType != ExpressionType.ConvertChecked)
				ec.ig.Emit (is_unsigned ? unsigned : signed);
			else
				ec.ig.Emit (is_unsigned ? unsigned_checked : signed_checked);
		}

		void EmitPrimitiveConversion (EmitContext ec)
		{
			operand.Emit (ec);

			EmitPrimitiveConversion (ec, operand.Type, Type);
		}

		void EmitPrimitiveConversion (EmitContext ec, Type from, Type to)
		{
			var is_unsigned = IsUnsigned (from);

			switch (Type.GetTypeCode (to)) {
			case TypeCode.SByte:
				EmitPrimitiveConversion (ec,
					is_unsigned,
					OpCodes.Conv_I1,
					OpCodes.Conv_U1,
					OpCodes.Conv_Ovf_I1,
					OpCodes.Conv_Ovf_I1_Un);
				return;
			case TypeCode.Byte:
				EmitPrimitiveConversion (ec,
					is_unsigned,
					OpCodes.Conv_I1,
					OpCodes.Conv_U1,
					OpCodes.Conv_Ovf_U1,
					OpCodes.Conv_Ovf_U1_Un);
				return;
			case TypeCode.Int16:
				EmitPrimitiveConversion (ec,
					is_unsigned,
					OpCodes.Conv_I2,
					OpCodes.Conv_U2,
					OpCodes.Conv_Ovf_I2,
					OpCodes.Conv_Ovf_I2_Un);
				return;
			case TypeCode.UInt16:
				EmitPrimitiveConversion (ec,
					is_unsigned,
					OpCodes.Conv_I2,
					OpCodes.Conv_U2,
					OpCodes.Conv_Ovf_U2,
					OpCodes.Conv_Ovf_U2_Un);
				return;
			case TypeCode.Int32:
				EmitPrimitiveConversion (ec,
					is_unsigned,
					OpCodes.Conv_I4,
					OpCodes.Conv_U4,
					OpCodes.Conv_Ovf_I4,
					OpCodes.Conv_Ovf_I4_Un);
				return;
			case TypeCode.UInt32:
				EmitPrimitiveConversion (ec,
					is_unsigned,
					OpCodes.Conv_I4,
					OpCodes.Conv_U4,
					OpCodes.Conv_Ovf_U4,
					OpCodes.Conv_Ovf_U4_Un);
				return;
			case TypeCode.Int64:
				EmitPrimitiveConversion (ec,
					is_unsigned,
					OpCodes.Conv_I8,
					OpCodes.Conv_U8,
					OpCodes.Conv_Ovf_I8,
					OpCodes.Conv_Ovf_I8_Un);
				return;
			case TypeCode.UInt64:
				EmitPrimitiveConversion (ec,
					is_unsigned,
					OpCodes.Conv_I8,
					OpCodes.Conv_U8,
					OpCodes.Conv_Ovf_U8,
					OpCodes.Conv_Ovf_U8_Un);
				return;
			case TypeCode.Single:
				if (is_unsigned)
					ec.ig.Emit (OpCodes.Conv_R_Un);
				ec.ig.Emit (OpCodes.Conv_R4);
				return;
			case TypeCode.Double:
				if (is_unsigned)
					ec.ig.Emit (OpCodes.Conv_R_Un);
				ec.ig.Emit (OpCodes.Conv_R8);
				return;
			default:
				throw new NotImplementedException (this.Type.ToString ());
			}
		}

		void EmitArithmeticUnary (EmitContext ec)
		{
			if (!IsLifted) {
				operand.Emit (ec);
				EmitUnaryOperator (ec);
			} else
				EmitLiftedUnary (ec);
		}

		void EmitUserDefinedLiftedToNullOperator (EmitContext ec)
		{
			var ig = ec.ig;
			var local = ec.EmitStored (operand);

			var ret = ig.DefineLabel ();
			var done = ig.DefineLabel ();

			ec.EmitNullableHasValue (local);
			ig.Emit (OpCodes.Brfalse, ret);

			ec.EmitNullableGetValueOrDefault (local);
			ec.EmitCall (method);
			ec.EmitNullableNew (Type);
			ig.Emit (OpCodes.Br, done);

			ig.MarkLabel (ret);

			var temp = ig.DeclareLocal (Type);
			ec.EmitNullableInitialize (temp);

			ig.MarkLabel (done);
		}

		void EmitUserDefinedLiftedOperator (EmitContext ec)
		{
			var local = ec.EmitStored (operand);
			ec.EmitNullableGetValue (local);
			ec.EmitCall (method);
		}

		void EmitUserDefinedOperator (EmitContext ec)
		{
			if (!IsLifted) {
				ec.Emit (operand);
				ec.EmitCall (method);
			} else if (IsLiftedToNull) {
				EmitUserDefinedLiftedToNullOperator (ec);
			} else
				EmitUserDefinedLiftedOperator (ec);
		}

		void EmitQuote (EmitContext ec)
		{
			ec.EmitScope ();

			ec.EmitReadGlobal (operand, typeof (Expression));

			if (ec.HasHoistedLocals)
				ec.EmitLoadHoistedLocalsStore ();
			else
				ec.ig.Emit (OpCodes.Ldnull);

			ec.EmitIsolateExpression ();
		}

		internal override void Emit (EmitContext ec)
		{
			if (method != null) {
				EmitUserDefinedOperator (ec);
				return;
			}

			switch (this.NodeType) {
			case ExpressionType.ArrayLength:
				EmitArrayLength (ec);
				return;
			case ExpressionType.TypeAs:
				EmitTypeAs (ec);
				return;
			case ExpressionType.Convert:
			case ExpressionType.ConvertChecked:
				EmitConvert (ec);
				return;
			case ExpressionType.Not:
			case ExpressionType.Negate:
			case ExpressionType.NegateChecked:
			case ExpressionType.UnaryPlus:
				EmitArithmeticUnary (ec);
				return;
			case ExpressionType.Quote:
				EmitQuote (ec);
				return;
			default:
				throw new NotImplementedException (this.NodeType.ToString ());
			}
		}
	}
}
