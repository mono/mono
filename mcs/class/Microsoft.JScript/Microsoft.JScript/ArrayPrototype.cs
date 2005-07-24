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
		
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject | JSFunctionAttributeEnum.HasEngine |
			JSFunctionAttributeEnum.HasVarArgs, JSBuiltin.Array_concat)]
		public static ArrayObject concat (object thisObj, VsaEngine engine,
						  params object [] args)
		{
			int i = 0;
			ArrayObject result = new ArrayObject ();
			int arg_idx = -1;
			int arg_count = args.Length;

			// TODO: Shouldn't this be generic!?
			SemanticAnalyser.assert_type (thisObj, typeof (ArrayObject));
			object cur_obj = thisObj;

			ArrayObject cur_ary;
			while (cur_obj != null) {
				if (cur_obj is ArrayObject) {
					cur_ary = (ArrayObject) cur_obj;

					int n = (int) cur_ary.length;
					for (int j = 0; j < n; j++, i++)
						result.elems [i] = cur_ary.elems [j];
				} else
					result.elems [i++] = cur_obj;

				arg_idx++;
				cur_obj = arg_idx < arg_count ? args [arg_idx] : null;
			}

			result.length = i;

			return result;
		}

		public static ArrayConstructor constructor {
			get { return ArrayConstructor.Ctr; }
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_join)]
		public static string join (object thisObj, object separator)
		{
			// TODO: Shouldn't this be generic!?
			SemanticAnalyser.assert_type (thisObj, typeof (ArrayObject));
			ArrayObject array_obj = (ArrayObject) thisObj;

			string _separator;
			if (separator == null)
				_separator = ",";
			else
				_separator = Convert.ToString (separator);

			Hashtable elems = array_obj.elems;
			int n = (int) array_obj.length;
			StringBuilder str = new StringBuilder ();
			bool first = true;

			for (int i = 0; i < n; i++) {
				if (!first)
					str.Append (_separator);
				first = false;
				object elem = elems [i];
				if (elem != null)
					str.Append (Convert.ToString (elem));
			}
			return str.ToString ();
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_pop)]
		public static object pop (object thisObj)
		{
			// TODO: Shouldn't this be generic!?
			SemanticAnalyser.assert_type (thisObj, typeof (ArrayObject));
			ArrayObject array_obj = (ArrayObject) thisObj;
			Hashtable elems = array_obj.elems;

			int n = (int) array_obj.length;
			if (n > 0) {
				int new_len = n - 1;
				array_obj.length = new_len;
				if (elems.ContainsKey (new_len)) {
					object result = elems [new_len];
					elems.Remove (new_len);
					return result;
				}
			}
			return null;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject | JSFunctionAttributeEnum.HasVarArgs, JSBuiltin.Array_push)]
		public static long push (object thisObj, params object [] args)
		{
			// TODO: Shouldn't this be generic!?
			SemanticAnalyser.assert_type (thisObj, typeof (ArrayObject));
			ArrayObject array_obj = (ArrayObject) thisObj;
			Hashtable elems = array_obj.elems;

			int i = (int) array_obj.length;
			int n = i + args.Length;

			for (int j = 0; i < n; i++, j++)
				elems [i] = args [j];

			array_obj.length = n;
			return n;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_reverse)]
		public static object reverse (object thisObj)
		{
			// TODO: Shouldn't this be generic!?
			SemanticAnalyser.assert_type (thisObj, typeof (ArrayObject));
			ArrayObject array_obj = (ArrayObject) thisObj;
			Hashtable elems = array_obj.elems;

			int n = (int) array_obj.length;
			int half_n = n / 2;
			int j = n - 1;
			object temp;
			
			for (int i = 0; i < half_n; i++, j--) {
				temp = elems [i];
				elems [i] = elems [j];
				elems [j] = temp;
			}

			return array_obj;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_shift)]
		public static object shift (object thisObj)
		{
			// TODO: Shouldn't this be generic!?
			SemanticAnalyser.assert_type (thisObj, typeof (ArrayObject));
			ArrayObject array_obj = (ArrayObject) thisObj;
			Hashtable elems = array_obj.elems;

			int n = (int) array_obj.length;
			if (n > 0) {
				array_obj.length = n - 1;
				if (elems.ContainsKey (0)) {
					object result = elems [0];
					elems.Remove (0);
					for (int i = 1; i < n; i++)
						elems [i - 1] = elems [i];
					elems.Remove (n - 1);
					return result;
				}
			}
			return null;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject | JSFunctionAttributeEnum.HasEngine, JSBuiltin.Array_slice)]
		public static ArrayObject slice (object thisObj, VsaEngine engine, double start, object end)
		{
			// TODO: Shouldn't this be generic!?
			SemanticAnalyser.assert_type (thisObj, typeof (ArrayObject));
			ArrayObject array_obj = (ArrayObject) thisObj;
			int array_len = (int) array_obj.length;
			int _start, _end;

			if (start > array_len)
				_start = array_len;
			else {
				_start = (int) start;
				if (_start < 0)
					_start += array_len;
			}

			if (end == null)
				_end = array_len;
			else {
				_end = Convert.ToInt32 (end);

				if (_end < 0)
					_end += array_len;
				else if (_end > array_len)
					_end = array_len;
			}

			if (_end < _start)
				_end = _start;

			ArrayObject result = new ArrayObject();
			result.length = _end - _start;

			for (int i = _start; i < _end; i++)
				result.elems [i - _start] = array_obj.elems [i];

			return result;
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
			// TODO: Shouldn't this be generic!?
			SemanticAnalyser.assert_type (thisObj, typeof (ArrayObject));
			ArrayObject array_obj = (ArrayObject) thisObj;
			ArrayObject result = new ArrayObject ();
			Hashtable elems = array_obj.elems;
			Hashtable del_elems = result.elems;

			int old_length = (int) array_obj.length;
			start = (int) start;
			if (start < 0)
				start = Math.Max (start + old_length, 0);
			else
				start = Math.Min (old_length, start);

			deleteCnt = (int) deleteCnt;
			deleteCnt = Math.Min (Math.Max (deleteCnt, 0), old_length - start);

			int arg_length = args.Length;
			int add_length = arg_length - (int) deleteCnt;
			add_length = Math.Max (add_length, -old_length);
			int del_length = -add_length;
			int new_length = old_length + add_length;

			int i, j, m;
			// First let's make some free space for the new items (if needed)
			if (add_length > 0) {
				i = old_length - 1;
				j = i + add_length;
				for (; i >= start; i--, j--)
					elems [j] = elems [i];
			}

			// Then insert the new items in the now free space / replace existing ones
			j = m = 0;
			int old_start = (int) start + add_length;
			for (i = (int) start; j < arg_length; i++, j++) {
				if (i >= old_start && elems.ContainsKey (i)) {
					del_elems [m] = elems [i];
					m++;
					elems.Remove (i);
				}

				if (j < arg_length)
					elems [i] = args [j];
			}

			// Finally, delete elements which have no replacement elements
			if (add_length < 0) {
				int last_elem_idx = i + del_length;
				for (int k = 0; k < del_length; i++, j++, k++) {
					if (elems.ContainsKey (i)) {
						del_elems [m] = elems [i];
						m++;
						elems.Remove (i);
					}
				}

				// And move up trailing elements
				int l = last_elem_idx - del_length;
				for (int k = last_elem_idx; l < old_length; k++, l++) {
					if (elems.ContainsKey (k))
						elems [l] = elems [k];
					else if (elems.ContainsKey (l))
						elems.Remove (l);
				}
			}

			array_obj.length = new_length;
			result.length = (int) deleteCnt;
			return result;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_toLocaleString)]
		public static string toLocaleString (object thisObj)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_toString)]
		public static string toString (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (ArrayObject));
			return ArrayPrototype.join (thisObj, null);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject | JSFunctionAttributeEnum.HasVarArgs, JSBuiltin.Array_unshift)]
		public static object unshift (object thisObj, params object [] args)
		{
			// TODO: Shouldn't this be generic!?
			SemanticAnalyser.assert_type (thisObj, typeof (ArrayObject));
			ArrayObject array_obj = (ArrayObject) thisObj;
			Hashtable elems = array_obj.elems;

			int old_length = (int) array_obj.length;
			int arg_length = args.Length;
			int new_length = old_length + arg_length;

			if (arg_length > 0) {
				// First let's make some free space for the new items
				int i = old_length - 1;
				int j = i + arg_length;
				for (; i >= 0; i--, j--)
					elems [j] = elems [i];

				// Then insert the new items in the now free space
				for (; j >= 0; j--)
					elems [j] = args [j];
			}
			//
			// NOTE: MSC returns the new array, but
			// ECMA-262 says to return the new length. We
			// conform to the standard.
			//
			array_obj.length = new_length;
			return new_length;
		}
	}
}
