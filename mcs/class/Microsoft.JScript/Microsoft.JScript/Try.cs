//
// Try.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	using System;
	using Microsoft.JScript.Vsa;

	public class Try : AST
	{
		public static Object JScriptExceptionValue (object e, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}

		public static void PushHandlerScope (VsaEngine engine, string id, int scopeId)
		{
			throw new NotImplementedException ();
		}


		internal override object Visit (Visitor v, object args)
		{
			return v.VisitTry (this, args);
		}
	}
}