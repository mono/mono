//
// Eval.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp
{
	using System;
	using Microsoft.JScript.Vsa;

	public class Eval : AST
	{
		public static object JScriptEvaluate (object src, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}


		internal override object Visit (Visitor v, object args)
		{	
			return v.VisitEval (this, args);
		}
	}
}