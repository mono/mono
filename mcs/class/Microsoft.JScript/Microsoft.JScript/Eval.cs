//
// Eval.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	using System;

	public class Eval : AST
	{
		public static object JScriptEvaluate (object src, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}


		public override object Visit (Visitor v, object args)
		{	
			return v.VisitEval (this, args);
		}
	}
}