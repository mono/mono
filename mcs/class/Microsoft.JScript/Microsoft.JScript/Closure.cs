//
// Closure.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren
//

using System;

namespace Microsoft.JScript {

	public class Closure : ScriptFunction {

		public Object args;
		public Object caller;

		public Closure (FunctionObject func)
		{
		}

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
	}
}
