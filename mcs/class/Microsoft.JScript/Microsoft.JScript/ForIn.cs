//
// ForIn.cs:
//
// Author: 
//	Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	using System;
	using System.Collections;

	public class ForIn : AST 
	{
		public static IEnumerator JScriptGetEnumerator (object coll)
		{
			throw new NotImplementedException ();
		}


		public override object Visit (Visitor v, object args)
		{
			return v.VisitForIn (this, args);
		}
	}
}