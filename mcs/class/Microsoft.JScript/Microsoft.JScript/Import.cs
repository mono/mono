//
// Import.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using Microsoft.JScript.Vsa;
using System;

namespace Microsoft.JScript.Tmp {

	public class Import : AST {

		string name;

		public static void JScriptImport (string name, VsaEngine engine)
		{}

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}
	}
}
