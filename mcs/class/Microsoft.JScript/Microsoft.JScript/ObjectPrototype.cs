//
// ObjectPrototype.cs
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript
{
	using System;

	public class ObjectPrototype : JSObject
	{
		public static ObjectConstructor constructor {
			get { throw new NotImplementedException (); }
		}


		public static bool hasOwnProperty (object thisObj, object name)
		{
			throw new NotImplementedException ();
		}


		public static bool isPrototypeOf (object thisObj, object obj)
		{
			throw new NotImplementedException ();
		}


		public static bool propertyIsEnumerable (object thisObj, object name)
		{
			throw new NotImplementedException ();
		}


		public static string toLocaleString (object thisObj)
		{
			throw new NotImplementedException ();
		}


		public static string toString (object thisObj)
		{
			throw new NotImplementedException ();
		}


		public static object valueOf (object thisObj)
		{
			throw new NotImplementedException ();
		}
	}
}