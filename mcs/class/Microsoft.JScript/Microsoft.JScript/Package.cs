//
// Package.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using Microsoft.JScript.Vsa;
using System.Collections;

namespace Microsoft.JScript.Tmp {

	public class Package : AST {

		internal string Name;
		internal ArrayList Members;

		public static void JScriptPackage (string rootName, VsaEngine engine)
		{}
	}
}