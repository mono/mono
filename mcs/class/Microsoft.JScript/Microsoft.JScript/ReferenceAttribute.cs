//
// ReferenceAttribute.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript
{
	using System;

	public class ReferenceAttribute : Attribute
	{
		public string reference;
		
		public ReferenceAttribute (string reference)
		{
			throw new NotImplementedException ();
		}
	}
}