//
// FunctionObject.cs:
//
// Author:
//	Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System;

namespace Microsoft.JScript.Tmp {

	public class FunctionObject : ScriptFunction {

		internal string Name;
		internal string ReturnType;
		internal FormalParameterList Params;
		internal Block Body;
	    
		internal FunctionObject ()
		{
			Params = new FormalParameterList ();
			Body = new Block ();
		}

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
	}
}
