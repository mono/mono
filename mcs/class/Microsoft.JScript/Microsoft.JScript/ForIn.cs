//
// ForIn.cs:
//
// Author: 
//	Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System;
using System.Collections;

namespace Microsoft.JScript.Tmp {

	public class ForIn : AST {

		public static IEnumerator JScriptGetEnumerator (object coll)
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
