//
// BooleanPrototype.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	using System;

	public class BooleanPrototype : BooleanObject
	{
		public static BooleanConstructor constructor {
			get { throw new NotImplementedException (); }
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