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
				operand.Emit (ec);
			else if (IsNullable (from) && !IsNullable (target))
				EmitConvertFromNullable (ec);
			else if (!IsNullable (from) && IsNullable (target))
				EmitConvertToNullable (ec);
			else if (IsNullable (from) && IsNullable (target))
				EmitConvertFromNullableToNullable (ec);
			else if (IsReferenceConversion (from, target))
				EmitCast (ec);
			else if (IsPrimitiveConversion (from, target))
				EmitPrimitiveConversion (ec);
			else
				throw new NotImplementedException ();
		}

		static Type MakeNullableType (Type type)
		{
			return typeof (Nullable<>).MakeGenericType (type);
		}

		void EmitConvertFromNullableToNullable (EmitContext ec)
		{
			var ig = ec.ig;

			var from = ec.EmitStored (operand);
			var to = ig.DeclareLocal (Type);

			var has_value = ig.DefineLabel ();
			var done = ig.DefineLabel ();

			ec.EmitNullableHasValue (from);
			ig.Emit (OpCodes.Brtrue, has_value);

			// if not has value
			ig.Emit (OpCodes.Ldloca, to);
			ig.Emit (OpCodes.Initobj, to.LocalType);
			ig.Emit (OpCodes.Ldloc, to);

			ig.Emit (OpCodes.Br, done);

			ig.MarkLabel (has_value);
			// if has value
			ec.EmitNullableGetValueOrDefault (from);
			ec.EmitNullableNew (to.LocalType);

			ig.MarkLabel (done);
		}

		void EmitConvertToNullable (EmitContext ec)
		{
			ec.Emit (operand);

			if (IsUnBoxing ()) {
				EmitUnbox (ec);
				return;
			}

			ec.EmitNullableNew (MakeNullableType (operand.Type));
		}

		void EmitConvertFromNullable (EmitContext ec)
		{
			if (IsBoxing ()) {
				ec.Emit (operand);
				EmitBox (ec);
				return;
			}

			ec.EmitCall (operand, operand.Type.GetMethod ("get_Value"));
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

		void EmitPrimitiveConversion (EmitContext ec,
			OpCode signed, OpCode unsigned, OpCode signed_checked, OpCode unsigned_checked)
		{
			operand.Emit (ec);

			bool is_unsigned = IsUnsigned (operand.Type);

			if (this.NodeType != ExpressionType.ConvertChecked)
				ec.ig.Emit (is_unsigned ? unsigned : signed);
			else
				ec.ig.Emit (is_unsigned ? unsigned_checked : signed_checked);
		}

		void EmitPrimitiveConversion (EmitContext ec)
		{
			switch (Type.GetTypeCode (this.Type)) {
			case TypeCode.SByte:
				EmitPrimitiveConversion (ec,
					OpCodes.Conv_I1,
					OpCodes.Conv_U1,
					OpCodes.Conv_Ovf_I1,
					OpCodes.Conv_Ovf_I1_Un);
				return;
			case TypeCode.Byte:
				EmitPrimitiveConversion (ec,
					OpCodes.Conv_I1,
					OpCodes.Conv_U1,
					OpCodes.Conv_Ovf_U1,
					OpCodes.Conv_Ovf_U1_Un);
				return;
			case TypeCode.Int16:
				EmitPrimitiveConversion (ec,
					OpCodes.Conv_I2,
					OpCodes.Conv_U2,
					OpCodes.Conv_Ovf_I2,
					OpCodes.Conv_Ovf_I2_Un);
				return;
			case TypeCode.UInt16:
				EmitPrimitiveConversion (ec,
					OpCodes.Conv_I2,
					OpCodes.Conv_U2,
					OpCodes.Conv_Ovf_U2,
					OpCodes.Conv_Ovf_U2_Un);
				return;
			case TypeCode.Int32:
				EmitPrimitiveConversion (ec,
					OpCodes.Conv_I4,
					OpCodes.Conv_U4,
					OpCodes.Conv_Ovf_I4,
					OpCodes.Conv_Ovf_I4_Un);
				return;
			case TypeCode.UInt32:
				EmitPrimitiveConversion (ec,
					OpCodes.Conv_I4,
					OpCodes.Conv_U4,
					OpCodes.Conv_Ovf_U4,
					OpCodes.Conv_Ovf_U4_Un);
				return;
			case TypeCode.Int64:
				EmitPrimitiveConversion (ec,
					OpCodes.Conv_I8,
					OpCodes.Conv_U8,
					OpCodes.Conv_Ovf_I8,
					OpCodes.Conv_Ovf_I8_Un);
				return;
			case TypeCode.UInt64:
				EmitPrimitiveConversion (ec,
					OpCodes.Conv_I8,
					OpCodes.Conv_U8,
					OpCodes.Conv_Ovf_U8,
					OpCodes.Conv_Ovf_U8_Un);
				return;
			case TypeCode.Single:
				if (IsUnsigned (operand.Type))
					ec.ig.Emit (OpCodes.Conv_R_Un);
				ec.ig.Emit (OpCodes.Conv_R4);
				return;
			case TypeCode.Double:
				if (IsUnsigned (operand.Type))
					ec.ig.Emit (OpCodes.Conv_R_Un);
				ec.ig.Emit (OpCodes.Conv_R8);
				return;
			default:
				throw new NotImplementedException (this.Type.ToString ());
			}
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
				EmitConvert (ec);
				return;
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
