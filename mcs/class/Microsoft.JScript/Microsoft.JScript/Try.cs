//
// Try.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren
//

using System;
using Microsoft.JScript.Vsa;

namespace Microsoft.JScript.Tmp {

	public class Try : AST {

		public static Object JScriptExceptionValue (object e, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}

		public static void PushHandlerScope (VsaEngine engine, string id, int scopeId)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}
	}
}
