//
// StringPrototype.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript
{
	using System;

	public class StringPrototype : StringObject
	{
		public static string anchor (object thisObj, object anchorName)
		{
			throw new NotImplementedException ();
		}


		public static string big (object thisObj)
		{
			throw new NotImplementedException ();
		}


		public static string blink (object thisObj)
		{
			throw new NotImplementedException ();
		}


		public static string bold (object thisObj)
		{
			throw new NotImplementedException ();
		}


		public static string charAt (object thisObj, double pos)
		{
			throw new NotImplementedException ();
		}


		public static object charCodeAt (object thisObj, double pos)
		{
			throw new NotImplementedException ();
		}


		public static string concat (object thisObj, params object [] args)
		{
			throw new NotImplementedException ();
		}


		public static StringConstructor constructor {
			get { throw new NotImplementedException (); }
		}


		public static string @fixed (object thisObj)
		{
			throw new NotImplementedException ();
		}


		public static string fontcolor (object thisObj, object colorName)
		{
			throw new NotImplementedException ();
		}


		public static string fontsize (object thisObj, object fontsize)
		{
			throw new NotImplementedException ();
		}


		public static int indexOf (object thisObj, object searchString, double position)
		{
			throw new NotImplementedException ();
		}


		public static string italics (object thisObj)
		{
			throw new NotImplementedException ();
		}


		public static int lastIndexOf (object thisobj, object searchString, double position)
		{
			throw new NotImplementedException ();
		}


		public static string link (object thisObj, object linkRef)
		{
			throw new NotImplementedException ();
		}


		public static int localeCompare (object thisObj, object thatObj)
		{
			throw new NotImplementedException ();
		}


		public static object match (object thisObj, VsaEngine engine, object regExp)
		{
			throw new NotImplementedException ();
		}


		public static string replace (object thisObj, object regExp, object replacement)
		{
			throw new NotImplementedException ();
		}


		public static int search (object thisObj, VsaEngine engine, object regExp)
		{
			throw new NotImplementedException ();
		}


		public static string slice (object thisObj, double start, object end)
		{
			throw new NotImplementedException ();
		}


		public static string small (object thisObj)
		{
			throw new NotImplementedException ();
		}


		public static ArrayObject split (object thisObj, VsaEngine engine,
						 object separator, object limit)
		{
			throw new NotImplementedException ();
		}


		public static string strike (object thisOBj)
		{
			throw new NotImplementedException ();
		}


		public static string sub (object thisObj)
		{
			throw new NotImplementedException ();
		}


		public static string substr (object thisObj, double start, object count)
		{
			throw new NotImplementedException ();
		}


		public static string substring (object thisObj, double start, object end)
		{
			throw new NotImplementedException ();
		}


		public static string sup (object thisObj)
		{
			throw new NotImplementedException ();
		}


		public static string toLocaleLowerCase (object thisObj)
		{
			throw new NotImplementedException ();
		}


		public static string toLocaleUpperCase (object thisObj)
		{
			throw new NotImplementedException ();
		}


		public static string toLowerCase (object thisObj)
		{
			throw new NotImplementedException ();
		}


		public static string toString (object thisObj)
		{
			throw new NotImplementedException ();
		}


		public static string toUpperCase (object thisObj)
		{
			throw new NotImplementedException ();
		}


		public static object valueOf (object thisObj)
		{
			throw new NotImplementedException ();
		}
	}
}