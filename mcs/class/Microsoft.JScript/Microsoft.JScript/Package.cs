//
// Package.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	public class Package : AST
	{
		public static void JScriptPackage (string rootName, VsaEngine engine)
		{}


		public override object Visit (Visitor v, object args)
		{	
			return v.VisitPackage (this, args);
		}
	}
}