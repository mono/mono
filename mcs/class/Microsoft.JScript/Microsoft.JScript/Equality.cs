//
// Equality.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;
using System.Text;
using System.Reflection.Emit;

namespace Microsoft.JScript {

	public class Equality : BinaryOp {

		internal Equality (AST parent, AST left, AST right, JSToken op)
		{
			this.left = left;
			this.right = right;
			this.current_op = op;
		}

		public Equality (Context context, AST oper1, AST oper2, JSToken operatorTok)
		{
			throw new NotImplementedException ();
		}

		public Equality (int i)
		{
			current_op = (JSToken) i;
		}

		public bool EvaluateEquality (object v1, object v2)
		{
			return false;
		}


		public static bool JScriptEquals (object v1, object v2)
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
			bool r = true;
			if (left != null)
				r &= left.Resolve (context);
			if (right != null)
				r &= right.Resolve (context);
			return r;
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			this.no_effect = no_effect;
			return  Resolve (context);
		}	       

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			LocalBuilder local_builder;

			if (current_op != JSToken.None) {
				Type t = typeof (Equality);				
				local_builder = ig.DeclareLocal (t);
				if (current_op == JSToken.Equal)
					ig.Emit (OpCodes.Ldc_I4_S, (byte) 53);
				else if (current_op == JSToken.NotEqual)
					ig.Emit (OpCodes.Ldc_I4_S, (byte) 54);
				ig.Emit (OpCodes.Newobj, t.GetConstructor (new Type [] {typeof (int)}));
				ig.Emit (OpCodes.Stloc, local_builder);
				ig.Emit (OpCodes.Ldloc, local_builder);
			}

			if (left != null)
				left.Emit (ec);
			if (right != null)
				right.Emit (ec);			       
			
			if (current_op == JSToken.Equal) {
				ig.Emit (OpCodes.Call, typeof (Equality).GetMethod ("EvaluateEquality"));
				Label t = ig.DefineLabel ();
				Label f = ig.DefineLabel ();
				ig.Emit (OpCodes.Brtrue_S, t);
				ig.Emit (OpCodes.Ldc_I4_0);
				ig.Emit (OpCodes.Br_S, f);
				ig.MarkLabel (t);
				ig.Emit (OpCodes.Ldc_I4_1);
				ig.MarkLabel (f);
			}

			if (no_effect)
				ig.Emit (OpCodes.Pop);
		}
	}
}
