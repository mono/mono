//
// Throw.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp
{
	using System;

	public class Throw : AST
	{
		public static Exception JScriptThrow (object value)
		{
			throw new NotImplementedException ();
		}


		internal override object Visit (Visitor v, object args)
		{
			return v.VisitThrow (this, args);
		}
	}
}