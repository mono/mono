//
// Closure.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren
//

namespace Microsoft.JScript
{
	using System;

	public class Closure : ScriptFunction
	{
		public Object args;
		public Object caller;

		public Closure (FunctionObject func)
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
	}
}