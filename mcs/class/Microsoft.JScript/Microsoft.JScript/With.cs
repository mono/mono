//
// With.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System;
using System.Text;
using Microsoft.JScript.Vsa;
using System.Reflection.Emit;

namespace Microsoft.JScript {

	public class With : AST {

		AST exp, stm;

		internal With (AST parent, AST exp, AST stm)
		{
			this.parent = parent;
			this.exp = exp;
			this.stm = stm;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append (exp.ToString () + " ");
			sb.Append (stm.ToString ());

			return sb.ToString ();
		}

		public static Object JScriptWith (object withObj, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;
			if (exp != null)
				r &= exp.Resolve (context);
			if (stm != null)
				r &= stm.Resolve (context);
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			if (exp != null)
				exp.Emit (ec);

			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
			ig.Emit (OpCodes.Call, typeof (With).GetMethod ("JScriptWith"));
			ig.Emit (OpCodes.Pop);

			ig.BeginExceptionBlock ();

			if (stm != null)
				stm.Emit (ec);

			ig.BeginFinallyBlock ();
			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
			ig.Emit (OpCodes.Call, typeof (VsaEngine).GetMethod ("PopScriptObject"));
			ig.Emit (OpCodes.Pop);			

			ig.EndExceptionBlock ();
		}
	}
}
