//
// BooleanConstructor.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp
{
	using System;

	public class BooleanConstructor : ScriptFunction
	{
		[JSFunctionAttribute(JSFunctionAttributeEnum.HasVarArgs)]	
		public new BooleanObject CreateInstance (params Object [] args)
		{
			throw new NotImplementedException ();
		}

		public bool Invoke (Object arg)
		{
			throw new NotImplementedException ();
		}
	}
}