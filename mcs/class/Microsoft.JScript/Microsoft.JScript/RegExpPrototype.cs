//
// RegExpPrototype.cs
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	using System;

	public class RegExpPrototype : JSObject
	{
		public static RegExpObject compile (object thisObj, object source, object flags)
		{
			throw new NotImplementedException ();
		}


		public static RegExpConstructor constructor {
			get { throw new NotImplementedException (); }
		}


		public static object exec (object thisObj, object input)
		{
			throw new NotImplementedException ();
		}


		public static bool test (object thisObj, object input)
		{
			throw new NotImplementedException ();
		}


		public static string toString (object thisObj)
		{
			throw new NotImplementedException ();
		}
	}
}