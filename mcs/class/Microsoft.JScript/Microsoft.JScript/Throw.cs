//
// Throw.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, 2004, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System;
using System.Reflection.Emit;

namespace Microsoft.JScript {

	public class Throw : AST {
	
		internal AST expression;

		internal Throw (AST exp)
		{
			expression = exp;
		}

		public static Exception JScriptThrow (object value)
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			return expression.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			return expression.Resolve (context);
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			expression.Emit (ec);
			ig.Emit (OpCodes.Call, typeof (Throw).GetMethod ("JScriptThrow"));
			ig.Emit (OpCodes.Throw);				
		}
	}
}
