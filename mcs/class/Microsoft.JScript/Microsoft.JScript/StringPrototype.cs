//
// StringPrototype.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace Microsoft.JScript.Tmp
{
	using System;
	using Microsoft.JScript.Vsa;

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