//
// ArrayConstructor.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp
{
	using System;

	public class ArrayConstructor : ScriptFunction
	{
		public ArrayObject ConstructArray (Object [] args)
		{
			throw new NotImplementedException ();
		}
		

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs)]
		public new ArrayObject CreateInstance (params Object [] args)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs)]
		public ArrayObject Invoke (params Object [] args)
		{
			throw new NotImplementedException ();
		}
	}
}