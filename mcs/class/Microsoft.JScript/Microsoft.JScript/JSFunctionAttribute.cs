//
// JSFunctionAttribute.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	using System;

	[AttributeUsage (AttributeTargets.Method |AttributeTargets.Constructor)]
	public class JSFunctionAttribute : Attribute
	{
		public JSFunctionAttribute (JSFunctionAttributeEnum value)
		{
			throw new NotImplementedException ();
		}

		public JSFunctionAttribute (JSFunctionAttributeEnum value, JSBuiltin builinFunction)
		{
			throw new NotImplementedException ();
		}

		public JSFunctionAttributeEnum GetAttributeValue ()
		{
			throw new NotImplementedException ();
		}
	}
}