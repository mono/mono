//
// Package.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp
{
	using Microsoft.JScript.Vsa;
	using System.Collections;

	public class Package : AST
	{
		internal string Name;
		internal ArrayList Members;

		public static void JScriptPackage (string rootName, VsaEngine engine)
		{}


		internal override object Visit (Visitor v, object args)
		{	
			return v.VisitPackage (this, args);
		}
	}
}