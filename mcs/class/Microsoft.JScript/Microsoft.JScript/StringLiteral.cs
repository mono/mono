//
// StringLiteral.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;

namespace Microsoft.JScript {

	internal class StringLiteral : AST {

		internal string str;

		public string Str {
			get { return str; }
			set { str = value; }
		}

		internal StringLiteral (string s)
		{
			str = s;
		}

		public override string ToString ()
		{
			return str;
		}

		internal override bool Resolve (IdentificationTable context)
		{
			return true;
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}
}
	
