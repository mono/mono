//
// VBArrayPrototype.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript
{
	using System;

	public class VBArrayPrototype : JSObject
	{
		public static VBArrayConstructor constructor {
			get { throw new NotImplementedException (); }
		}


		public static int dimensions (object thisObj)
		{
			throw new NotImplementedException ();
		}


		public static object getItem (object thisObj, params object [] args)
		{
			throw new NotImplementedException ();
		}


		public static int lbound (object thisObj, object dimension)
		{
			throw new NotImplementedException ();
		}


		public static ArrayObject toArray (object thisObj, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}


		public static int ubound (object thisObj, object dimension)
		{
			throw new NotImplementedException ();
		}
	}
}