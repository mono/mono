//
// ErrorConstructor.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp
{
	using System;

	public enum ErrorType : int {
		OtherError,
		EvalError,
		RangeError,
		ReferenceError,
		SyntaxError,
		TypeError,
		URIError
	};

	public class ErrorConstructor : ScriptFunction
	{
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs)]
		public new ErrorObject CreateInstance (params Object [] args)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute(JSFunctionAttributeEnum.HasVarArgs)]
		public Object Invoke (params Object [] args)
		{
			throw new NotImplementedException ();
		}
	}
}