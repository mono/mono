//
// Relational.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace Microsoft.JScript {

	public class Relational : BinaryOp {

		internal Relational (AST parent, AST left, AST right, JSToken op)
		{
			this.parent = parent;
			this.left = left;
			this.right = right;
			this.current_op = op;
		}
		
		public Relational (int operatorTok)
		{
			this.current_op = (JSToken) operatorTok;
		}

		public double EvaluateRelational (object v1, object v2)
		{
			return -1;
		}


		public static double JScriptCompare (object v1, object v2)
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append (left.ToString ());

			if (current_op != JSToken.None)
				sb.Append (current_op + " ");

			if (right != null)
				sb.Append (right.ToString ());

			return sb.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			if (left != null)
				left.Resolve (context);

			if (right != null)
				right.Resolve (context);

			return true;			
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (context);
		}

		internal override void Emit (EmitContext ec)
		{
			if (current_op == JSToken.None &&  right == null) {
				left.Emit (ec);
				return;
			}			

			Type t = typeof (Relational);						
			ILGenerator ig = ec.ig;

			LocalBuilder loc = ig.DeclareLocal (t);			
			ConstructorInfo ctr_info;
			
			switch (current_op) {
			case JSToken.GreaterThan:
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 57);
				break;
			case JSToken.LessThan:
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 58);
				break;
			case JSToken.LessThanEqual:
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 59);
				break;
			case JSToken.GreaterThanEqual:
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 60);
				break;
			}
			
			ctr_info = typeof (Relational).GetConstructor (new Type [] { typeof (Int32) });
			ig.Emit (OpCodes.Newobj, ctr_info);
			ig.Emit (OpCodes.Stloc, loc);
			ig.Emit (OpCodes.Ldloc, loc);
			
			if (left != null)
				left.Emit (ec);
			if (right != null)
				right.Emit (ec);

			ig.Emit (OpCodes.Call, t.GetMethod ("EvaluateRelational"));
			ig.Emit (OpCodes.Ldc_I4_0);
			ig.Emit (OpCodes.Conv_R8);

			Label a, b;
			a = ig.DefineLabel ();
			b = ig.DefineLabel ();

			switch (current_op) {
			case JSToken.GreaterThan:
				ig.Emit (OpCodes.Bgt_S, a);
				break;
			case JSToken.LessThan:
				ig.Emit (OpCodes.Blt_S, a);
				break;
			case JSToken.LessThanEqual:
				ig.Emit (OpCodes.Ble_S, a);
				break;
			case JSToken.GreaterThanEqual:
				ig.Emit (OpCodes.Bge_S, a);
				break;
			}			

			ig.Emit (OpCodes.Ldc_I4_0);
			ig.Emit (OpCodes.Br_S, b);
			ig.MarkLabel (a);
			ig.Emit (OpCodes.Ldc_I4_1);
			ig.MarkLabel (b);

			if (no_effect)				
				ig.Emit (OpCodes.Pop);
		}
	}
}
