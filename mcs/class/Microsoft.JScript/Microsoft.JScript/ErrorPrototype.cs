//
// ErrorPrototype.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript
{
	using System;

	public class ErrorPrototype : JSObject
	{
		public readonly string name;

		public ErrorConstructor constructor {
			get { throw new NotImplementedException (); }
		}


		public static string toString (object thisObj)
		{
			throw new NotImplementedException ();
		}
	}
}