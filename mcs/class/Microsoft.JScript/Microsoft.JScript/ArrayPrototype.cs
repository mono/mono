//
// ArrayPrototype.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript
{
	using System;

	public class ArrayPrototype : ArrayObject
	{
		public static ArrayObject concat (object thisObj, VsaEngine engine,
						  params object [] args)
		{
			throw new NotImplementedException ();
		}


		public static ArrayConstructor constructor {
			get { throw new NotImplementedException (); }
		}


		public static string join (object thisObj, object separator)
		{
			throw new NotImplementedException ();
		}


		public static object pop (object thisObj)
		{
			throw new NotImplementedException ();
		}


		public static long push (object thisObj, params object [] args)
		{
			throw new NotImplementedException ();
		}


		public static object reverse (object thisObj)
		{
			throw new NotImplementedException ();
		}


		public static object shift (object thisObj)
		{
			throw new NotImplementedException ();
		}


		public static ArrayObject slice (object thisObj, VsaEngine engine,
						 double start, object end)
		{
			throw new NotImplementedException ();
		}


		public static object sort (object thisObj, object function)
		{
			throw new NotImplementedException ();
		}


		public static ArrayObject splice (object thisObj, VsaEngine engine,
						  double start, double deleteCnt, 
						  params object [] args)
		{
			throw new NotImplementedException ();
		}


		public static string toLocaleString (object thisObj)
		{
			throw new NotImplementedException ();
		}


		public static string ToString (object thisObj)
		{
			throw new NotImplementedException ();
		}


		public static object unshift (object thisObj, params object [] args)
		{
			throw new NotImplementedException ();
		}
	}
}