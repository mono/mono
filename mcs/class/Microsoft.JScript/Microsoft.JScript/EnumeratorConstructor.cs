//
// EnumeratorConstructor.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp
{
	using System;

	public class EnumeratorConstructor : ScriptFunction
	{
		[JSFunctionAttribute(JSFunctionAttributeEnum.HasVarArgs)]
		public new EnumeratorObject CreateInstance (params Object [] args)
		{
			throw new NotImplementedException ();
		}

		public Object Invoke ()
		{
			throw new NotImplementedException ();
		}
	}
}