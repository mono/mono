//
// FunctionConstructor.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	using System;

	public class FunctionConstructor : ScriptFunction 
	{
		[JSFunctionAttribute(JSFunctionAttributeEnum.HasVarArgs)]
		public new ScriptFunction CreateInstance (params Object [] args)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute(JSFunctionAttributeEnum.HasVarArgs)]
		public ScriptFunction Invoke(params Object [] args)
		{
			throw new NotImplementedException ();
		}
	}
}