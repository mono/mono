//
// Print.cs: The AST representation of a print_statement.
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;

namespace Microsoft.JScript {

	public class Print : AST {

		internal AST exp;

		public AST Exp {
			get { return exp; }
			set { exp = value; }
		}

		internal Print ()
		{}

		public override string ToString ()
		{
			return exp.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}
	}
}
