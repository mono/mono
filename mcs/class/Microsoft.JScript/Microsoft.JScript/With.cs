//
// With.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp
{
	using System;
	using Microsoft.JScript.Vsa;

	public class With : AST
	{
		public static Object JScriptWith (object withObj, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}


		internal override object Visit (Visitor v, object args)
		{
			return v.VisitWith (this, args);
		}
	}
}