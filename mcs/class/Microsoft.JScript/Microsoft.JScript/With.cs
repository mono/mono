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

namespace Microsoft.JScript {

	public class With : AST {

		AST exp, stm;

		internal With (AST exp, AST stm)
		{
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
			throw new NotImplementedException ();
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}
}
