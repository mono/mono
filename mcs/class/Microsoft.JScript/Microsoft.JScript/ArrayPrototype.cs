//
// ArrayPrototype.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
// (C) 2005, Novell Inc, (http://novell.com)
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

using System;
using System.Collections;
using System.Text;
using Microsoft.JScript.Vsa;

namespace Microsoft.JScript {

	public class ArrayPrototype : ArrayObject {
		
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject | JSFunctionAttributeEnum.HasVarArgs | JSFunctionAttributeEnum.HasEngine, JSBuiltin.Array_concat)]
		public static ArrayObject concat (object thisObj, VsaEngine engine,
						  params object [] args)
		{
			throw new NotImplementedException ();
		}


		public static ArrayConstructor constructor {
			get { return ArrayConstructor.Ctr; }
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_join)]
		public static string join (object thisObj, object separator)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (ArrayObject));
			ArrayObject array_obj = (ArrayObject) thisObj;
			
			string _separator;
			if (separator == null)
				_separator = ",";
			else 
				_separator = Convert.ToString (separator);

			Hashtable elems = array_obj.Elements;
			StringBuilder str = new StringBuilder ();
			bool first = true;
			foreach (DictionaryEntry entry in elems) {
				if (!first)
					str.Append (_separator);
				first = false;
				str.Append (Convert.ToString (entry.Value));
			}
			return str.ToString ();
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_pop)]
		public static object pop (object thisObj)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject | JSFunctionAttributeEnum.HasVarArgs, JSBuiltin.Array_push)]
		public static long push (object thisObj, params object [] args)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_reverse)]
		public static object reverse (object thisObj)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_shift)]
		public static object shift (object thisObj)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject | JSFunctionAttributeEnum.HasEngine, JSBuiltin.Array_slice)]
		public static ArrayObject slice (object thisObj, VsaEngine engine,
						 double start, object end)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_sort)]
		public static object sort (object thisObj, object function)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject | JSFunctionAttributeEnum.HasVarArgs | JSFunctionAttributeEnum.HasEngine, JSBuiltin.Array_splice)]
		public static ArrayObject splice (object thisObj, VsaEngine engine,
						  double start, double deleteCnt, 
						  params object [] args)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_toLocaleString)]
		public static string toLocaleString (object thisObj)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_toString)]
		public static string toString (object thisObj)
		{
			return ArrayPrototype.join (thisObj, null);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject | JSFunctionAttributeEnum.HasVarArgs, JSBuiltin.Array_unshift)]
		public static object unshift (object thisObj, params object [] args)
		{
			throw new NotImplementedException ();
		}
	}
}
