//
// FunctionObject.cs:
//
// Author:
//	Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp
{
	using System;

	public class FunctionObject : ScriptFunction
	{
		internal string Name;
		internal string ReturnType;
		internal FormalParameterList Params;
		internal Block Body;
	    
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
	}
}
