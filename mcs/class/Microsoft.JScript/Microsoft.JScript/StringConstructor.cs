//
// StringConstructor.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	using System;

	public class StringConstructor : ScriptFunction
	{
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs)]
		public new StringObject CreateInstance (params Object [] args)
		{
			throw new NotImplementedException ();
		}

		public string Invoke (Object arg)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs, JSBuiltin.String_fromCharCode)]
		public static String fromCharCode (params Object [] args)
		{
			throw new NotImplementedException ();
		}
	}
}