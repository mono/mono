//
// ArrayPrototype.cs:
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