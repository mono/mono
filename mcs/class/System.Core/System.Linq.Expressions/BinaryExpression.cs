//
// BinaryExpression.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//   Miguel de Icaza (miguel@novell.com)
//
// Contains code from the Mono C# compiler:
//   Marek Safar (marek.safar@seznam.cz)
//   Martin Baulig (martin@ximian.com)
//   Raja Harinath (harinath@gmail.com)
//
// (C) 2001-2003 Ximian, Inc.
// (C) 2004-2008 Novell, Inc. (http://www.novell.com)
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

	public sealed class BinaryExpression : Expression {

		Expression left;
		Expression right;
		LambdaExpression conversion;
		MethodInfo method;
		bool lift_to_null, is_lifted;

		public Expression Left {
			get { return left; }
		}

		public Expression Right {
			get { return right; }
		}

		public MethodInfo Method {
			get { return method; }
		}

		public bool IsLifted {
			get { return is_lifted;  }
		}

		public bool IsLiftedToNull {
			get { return lift_to_null; }
		}

		public LambdaExpression Conversion {
			get { return conversion; }
		}

		internal BinaryExpression (ExpressionType node_type, Type type, Expression left, Expression right)
			: base (node_type, type)
		{
			this.left = left;
			this.right = right;
		}

		internal BinaryExpression (ExpressionType node_type, Type type, Expression left, Expression right, MethodInfo method)
			: base (node_type, type)
		{
			this.left = left;
			this.right = right;
			this.method = method;
		}

		internal BinaryExpression (ExpressionType node_type, Type type, Expression left, Expression right, bool lift_to_null,
					   bool is_lifted, MethodInfo method, LambdaExpression conversion) : base (node_type, type)
		{
			this.left = left;
			this.right = right;
			this.method = method;
			this.conversion = conversion;
			this.lift_to_null = lift_to_null;
			this.is_lifted = is_lifted;
		}

		void EmitMethod (EmitContext ec)
		{
			left.Emit (ec);
			right.Emit (ec);
			ec.EmitCall (method);
		}

		static MethodInfo GetMethodNoPar (Type t, string name)
		{
			var method = t.GetMethod (name, Type.EmptyTypes);
			if (method == null)
				throw new ArgumentException (
					string.Format ("Internal error: method {0} with no parameters not found on {1}", name, t));

			return method;
		}

		void EmitArrayAccess (EmitContext ec)
		{
			left.Emit (ec);
			right.Emit (ec);
			ec.ig.Emit (OpCodes.Ldelem, this.Type);
		}

		void EmitLiftedLogical (EmitContext ec, bool and, bool short_circuit)
		{
			var ig = ec.ig;
			LocalBuilder ret = ig.DeclareLocal (Type);
			LocalBuilder vleft = null, vright = null;
			MethodInfo has_value = left.Type.GetMethod ("get_HasValue");
			MethodInfo get_value = GetMethodNoPar (left.Type, "get_Value");

			vleft = ec.EmitStored (left);
			if (!short_circuit)
				vright = ec.EmitStored (right);

			Label left_is_null = ig.DefineLabel ();
			Label right_is_null = ig.DefineLabel ();
			Label create = ig.DefineLabel ();
			Label exit = ig.DefineLabel ();
			Label both_are_null = ig.DefineLabel ();

			// Check left

			ig.Emit (OpCodes.Ldloca, vleft);
			ig.Emit (OpCodes.Call, has_value);
			ig.Emit (OpCodes.Brfalse, left_is_null);

			ig.Emit (OpCodes.Ldloca, vleft);
			ig.Emit (OpCodes.Call, get_value);
			ig.Emit (OpCodes.Dup);

			ig.Emit (and ? OpCodes.Brfalse : OpCodes.Brtrue, create);

			// Deal with right
			if (short_circuit)
				vright = ec.EmitStored (right);

			ig.Emit (OpCodes.Ldloca, vright);
			ig.Emit (OpCodes.Call, has_value);
			ig.Emit (OpCodes.Brfalse, right_is_null);

			ig.Emit (OpCodes.Ldloca, vright);
			ig.Emit (OpCodes.Call, get_value);

			ig.Emit (and ? OpCodes.And : OpCodes.Or);
			ig.Emit (OpCodes.Br, create);

			// left_is_null:
			ig.MarkLabel (left_is_null);

			ig.Emit (OpCodes.Ldloca, vright);
			ig.Emit (OpCodes.Call, has_value);
			ig.Emit (OpCodes.Brfalse, both_are_null);
			ig.Emit (OpCodes.Ldloca, vright);
			ig.Emit (OpCodes.Call, get_value);
			ig.Emit (OpCodes.Dup);
			ig.Emit (and ? OpCodes.Brfalse : OpCodes.Brtrue, create);

			// right_is_null:
			ig.MarkLabel (right_is_null);
			ig.Emit (OpCodes.Pop);

			// both_are_null:
			ig.MarkLabel (both_are_null);
			ig.Emit (OpCodes.Ldloca, ret);
			ig.Emit (OpCodes.Initobj, Type);
			ig.Emit (OpCodes.Ldloc, ret);
			ig.Emit (OpCodes.Br, exit);

			// create:
			ig.MarkLabel (create);
			ig.Emit (OpCodes.Newobj, Type.GetConstructors () [0]);

			// exit:
			ig.MarkLabel (exit);
		}

		void EmitLogical (EmitContext ec, bool and, bool short_circuit)
		{
			if (IsLifted) {
				EmitLiftedLogical (ec, and, short_circuit);
				return;
			}

			left.Emit (ec);
			right.Emit (ec);
			ec.ig.Emit (and ? OpCodes.And : OpCodes.Or);
		}

		void EmitCoalesce (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			LocalBuilder vleft;
			LocalBuilder vright;

			MethodInfo has_value = left.Type.GetMethod ("get_HasValue");

			Label exit = ig.DefineLabel ();
			Label try_right = ig.DefineLabel ();
			Label setup_null = ig.DefineLabel ();

			vleft = ec.EmitStored (left);
			if (IsNullable (left.Type)){
				ig.Emit (OpCodes.Ldloca, vleft);
				ig.Emit (OpCodes.Call, has_value);
			} else
				ig.Emit (OpCodes.Ldloc, vleft);

			ig.Emit (OpCodes.Brfalse, try_right);
			ig.Emit (OpCodes.Ldloc, vleft);
			ig.Emit (OpCodes.Br, exit);

		// try_right;
			ig.MarkLabel (try_right);
			vright = ec.EmitStored (right);
			if (IsNullable (right.Type)){
				ig.Emit (OpCodes.Ldloca, vright);
				ig.Emit (OpCodes.Call, has_value);
			} else
				ig.Emit (OpCodes.Ldloc, vright);

			ig.Emit (OpCodes.Brfalse, setup_null);
			ig.Emit (OpCodes.Ldloc, vright);
			ig.Emit (OpCodes.Br, exit);

		// setup_null:
			ig.MarkLabel (setup_null);
			LocalBuilder ret = ig.DeclareLocal (Type);
			ig.Emit (OpCodes.Ldloca, ret);
			ig.Emit (OpCodes.Initobj, Type);
			ig.Emit (OpCodes.Ldloc, ret);

		// exit:
			ig.MarkLabel (exit);
		}

		void EmitBinaryOperator (EmitContext ec)
		{
			OpCode opcode;
			var ig = ec.ig;
			bool is_unsigned = IsUnsigned (left.Type);

			switch (NodeType) {
			case ExpressionType.Add:
				opcode = OpCodes.Add;
				break;

			case ExpressionType.AddChecked:
				if (left.Type == typeof (int) || left.Type == typeof (long))
					opcode = OpCodes.Add_Ovf;
				else if (is_unsigned)
					opcode = OpCodes.Add_Ovf_Un;
				else
					opcode = OpCodes.Add;
				break;

			case ExpressionType.Subtract:
				opcode = OpCodes.Sub;
				break;

			case ExpressionType.SubtractChecked:
				if (left.Type == typeof (int) || left.Type == typeof (long))
					opcode = OpCodes.Sub_Ovf;
				else if (is_unsigned)
					opcode = OpCodes.Sub_Ovf_Un;
				else
					opcode = OpCodes.Sub;
				break;

			case ExpressionType.Multiply:
				opcode = OpCodes.Mul;
				break;

			case ExpressionType.MultiplyChecked:
				if (left.Type == typeof (int) || left.Type == typeof (long))
					opcode = OpCodes.Mul_Ovf;
				else if (is_unsigned)
					opcode = OpCodes.Mul_Ovf_Un;
				else
					opcode = OpCodes.Mul;
				break;

			case ExpressionType.Divide:
				if (is_unsigned)
					opcode = OpCodes.Div_Un;
				else
					opcode = OpCodes.Div;
				break;

			case ExpressionType.Modulo:
				if (is_unsigned)
					opcode = OpCodes.Rem_Un;
				else
					opcode = OpCodes.Rem;
				break;

			case ExpressionType.RightShift:
				if (is_unsigned)
					opcode = OpCodes.Shr_Un;
				else
					opcode = OpCodes.Shr;
				break;

			case ExpressionType.LeftShift:
				opcode = OpCodes.Shl;
				break;

			case ExpressionType.And:
				opcode = OpCodes.And;
				break;

			case ExpressionType.Or:
				opcode = OpCodes.Or;
				break;

			case ExpressionType.ExclusiveOr:
				opcode = OpCodes.Xor;
				break;

			case ExpressionType.GreaterThan:
				if (is_unsigned)
					opcode = OpCodes.Cgt_Un;
				else
					opcode = OpCodes.Cgt;
				break;

			case ExpressionType.GreaterThanOrEqual:
				Type le = left.Type;

				if (is_unsigned || (le == typeof (double) || le == typeof (float)))
					ig.Emit (OpCodes.Clt_Un);
				else
					ig.Emit (OpCodes.Clt);

				ig.Emit (OpCodes.Ldc_I4_0);

				opcode = OpCodes.Ceq;
				break;

			case ExpressionType.LessThan:
				if (is_unsigned)
					opcode = OpCodes.Clt_Un;
				else
					opcode = OpCodes.Clt;
				break;

			case ExpressionType.LessThanOrEqual:
				Type lt = left.Type;

				if (is_unsigned || (lt == typeof (double) || lt == typeof (float)))
					ig.Emit (OpCodes.Cgt_Un);
				else
					ig.Emit (OpCodes.Cgt);
				ig.Emit (OpCodes.Ldc_I4_0);

				opcode = OpCodes.Ceq;
				break;

			case ExpressionType.Equal:
				opcode = OpCodes.Ceq;
				break;

			case ExpressionType.NotEqual:
				ig.Emit (OpCodes.Ceq);
				ig.Emit (OpCodes.Ldc_I4_0);

				opcode = OpCodes.Ceq;
				break;

			default:
				throw new InvalidOperationException (string.Format ("Internal error: BinaryExpression contains non-Binary nodetype {0}", NodeType));
			}

			ig.Emit (opcode);
		}

		void EmitLiftedSimpleBinary (EmitContext ec)
		{
			Label empty_value;
			LocalBuilder ret = null;

			LocalBuilder vleft, vright;

			var ig = ec.ig;
			empty_value = ig.DefineLabel ();
			ret = ig.DeclareLocal (Type);

			vleft = ec.EmitStored (left);
			vright = ec.EmitStored (right);

			MethodInfo has_value = left.Type.GetMethod ("get_HasValue");
			MethodInfo get_value = GetMethodNoPar (left.Type, "get_Value");

			ig.Emit (OpCodes.Ldloca, vleft);
			ig.Emit (OpCodes.Call, has_value);
			ig.Emit (OpCodes.Brfalse, empty_value);
			ig.Emit (OpCodes.Ldloca, vright);
			ig.Emit (OpCodes.Call, has_value);
			ig.Emit (OpCodes.Brfalse, empty_value);
			ig.Emit (OpCodes.Ldloca, vleft);
			ig.Emit (OpCodes.Call, get_value);
			ig.Emit (OpCodes.Ldloca, vright);
			ig.Emit (OpCodes.Call, get_value);

			EmitBinaryOperator (ec);

			ig.Emit (OpCodes.Newobj, left.Type.GetConstructors () [0]);

			Label skip = ig.DefineLabel ();
			ig.Emit (OpCodes.Br_S, skip);
			ig.MarkLabel (empty_value);
			ig.Emit (OpCodes.Ldloc, ret);
			ig.Emit (OpCodes.Ldloca, ret);
			ig.Emit (OpCodes.Initobj, Type);

			ig.MarkLabel (skip);
		}

		void EmitSimpleBinary (EmitContext ec)
		{
			if (IsLifted) {
				EmitLiftedSimpleBinary (ec);
				return;
			}

			left.Emit (ec);
			right.Emit (ec);
			EmitBinaryOperator (ec);
		}

		internal override void Emit (EmitContext ec)
		{
			if (method != null){
				EmitMethod (ec);
				return;
			}

			switch (NodeType){
			case ExpressionType.ArrayIndex:
				EmitArrayAccess (ec);
				return;

			case ExpressionType.And:
				EmitLogical (ec, true, false);
				return;

			case ExpressionType.Or:
				EmitLogical (ec, false, false);
				return;

			case ExpressionType.AndAlso:
				EmitLogical (ec, true, true);
				return;

			case ExpressionType.OrElse:
				EmitLogical (ec, false, true);
				return;

			case ExpressionType.Coalesce:
				EmitCoalesce (ec);
				return;

			case ExpressionType.Power:
				left.Emit (ec);
				right.Emit (ec);
				ec.EmitCall (typeof (Math).GetMethod ("Pow"));
				return;

			default:
				EmitSimpleBinary (ec);
				return;
			}
		}
	}
}
