//
// FunctionPrototype.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	using System;

	public class FunctionPrototype : ScriptFunction
	{
		public static object apply (object thisObj, object thisArg, object argArray)
		{
			throw new NotImplementedException ();
		}


		public static object call (object thisObj, object thisArg, params object [] args)
		{
			throw new NotImplementedException ();
		}


		public static FunctionConstructor constructor {
			get { throw new NotImplementedException (); }
		}


		public static string toString (object thisObj)
		{
			throw new NotImplementedException ();
		}
	}
}