//
// NumberPrototype.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	using System;

	public class NumberPrototype : NumberObject
	{
		public static NumberConstructor constructor {
			get { throw new NotImplementedException (); }
		}


		public static string toExponential (object thisObj, object fractionDigits)
		{
			throw new NotImplementedException ();
		}


		public static string toFixed (object thisObj, double fractionDigits)
		{
			throw new NotImplementedException ();
		}


		public static string toLocalString (object thisObj)
		{
			throw new NotImplementedException ();
		}


		public static string toPrecision (object thisObj, object precision)
		{
			throw new NotImplementedException ();
		}


		public static string toString (object thisObj, object radix)
		{
			throw new NotImplementedException ();
		}


		public static object valueOf (object thisObj)
		{
			throw new NotImplementedException ();
		}
	}
}