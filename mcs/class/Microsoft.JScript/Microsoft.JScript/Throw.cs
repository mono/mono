//
// Throw.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System;

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
			throw new NotImplementedException ();
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}
}
