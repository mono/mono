//
// Package.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using Microsoft.JScript.Vsa;
using System.Collections;
using System;

namespace Microsoft.JScript {

	public class Package : AST {

		internal string Name;
		internal ArrayList Members;

		public static void JScriptPackage (string rootName, VsaEngine engine)
		{}

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
