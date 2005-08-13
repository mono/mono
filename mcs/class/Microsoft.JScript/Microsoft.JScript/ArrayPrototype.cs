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
using System.Globalization;

namespace Microsoft.JScript {

	public class ArrayPrototype : ArrayObject {
		
		internal static ArrayPrototype Proto = new ArrayPrototype ();

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject | JSFunctionAttributeEnum.HasEngine |
			JSFunctionAttributeEnum.HasVarArgs, JSBuiltin.Array_concat)]
		public static ArrayObject concat (object thisObj, VsaEngine engine,
						  params object [] args)
		{
			uint i = 0;
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

					uint n = (uint) cur_ary.length;
					for (uint j = 0; j < n; j++, i++)
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
			uint n = (uint) array_obj.length;
			StringBuilder str = new StringBuilder ();
			bool first = true;

			for (uint i = 0; i < n; i++) {
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

			uint n = (uint) array_obj.length;
			object result = null;
			if (n > 0) {
				uint new_len = n - 1;
				if (elems.ContainsKey (new_len))
					result = elems [new_len];
				// Element gets removed automatically
				array_obj.length = new_len;
			}
			return result;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject | JSFunctionAttributeEnum.HasVarArgs, JSBuiltin.Array_push)]
		public static long push (object thisObj, params object [] args)
		{
			// TODO: Shouldn't this be generic!?
			SemanticAnalyser.assert_type (thisObj, typeof (ArrayObject));
			ArrayObject array_obj = (ArrayObject) thisObj;
			Hashtable elems = array_obj.elems;

			uint i = (uint) array_obj.length;
			long n = i + args.Length;

			for (uint j = 0; i < n; i++, j++)
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

			uint n = (uint) array_obj.length;
			uint half_n = n / 2;
			uint j = n - 1;
			object temp;
			
			for (uint i = 0; i < half_n; i++, j--) {
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

			uint n = (uint) array_obj.length;
			object result = null;
			if (n > 0) {
				if (elems.ContainsKey ((uint) 0)) {
					result = elems [(uint) 0];
					elems.Remove ((uint) 0);
					for (uint i = 1; i < n; i++)
						elems [i - 1] = elems [i];
				}
				// Last element gets removed automatically
				array_obj.length = n - 1;
			}
			return result;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject | JSFunctionAttributeEnum.HasEngine, JSBuiltin.Array_slice)]
		public static ArrayObject slice (object thisObj, VsaEngine engine, double start, object end)
		{
			// TODO: Shouldn't this be generic!?
			SemanticAnalyser.assert_type (thisObj, typeof (ArrayObject));
			ArrayObject array_obj = (ArrayObject) thisObj;
			uint array_len = (uint) array_obj.length;
			uint _start, _end;

			if (start > array_len)
				_start = array_len;
			else {
				_start = (uint) start;
				if (_start < 0)
					_start += array_len;
			}

			if (end == null)
				_end = array_len;
			else {
				_end = Convert.ToUint32 (end);

				if (_end < 0)
					_end += array_len;
				else if (_end > array_len)
					_end = array_len;
			}

			if (_end < _start)
				_end = _start;

			ArrayObject result = new ArrayObject();
			result.length = _end - _start;

			for (uint i = _start; i < _end; i++)
				result.elems [i - _start] = array_obj.elems [i];

			return result;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_sort)]
		public static object sort (object thisObj, object function)
		{
			// TODO: Shouldn't this be generic?
			SemanticAnalyser.assert_type (thisObj, typeof (ArrayObject));
			ArrayObject array_obj = (ArrayObject) thisObj;
			ScriptFunction fun = function as ScriptFunction;
			uint n = (uint) array_obj.length;
			if (n > 1)
				SortHelper.qsort (array_obj.elems, 0, n - 1, SortHelper.CompareDelegateFor (fun));
			return array_obj;
		}

		private class SortHelper {
			internal delegate sbyte CompareDelegate (Hashtable elems, bool b1, object o1, uint i2);
			private ScriptFunction cmp;

			internal static CompareDelegate CompareDelegateFor (ScriptFunction fun) {
				if (fun == null)
					return new CompareDelegate (SortHelper.nativeCompare);

				SortHelper helper = new SortHelper (fun);
				return new CompareDelegate (helper.userCompare);
			}

			internal SortHelper (ScriptFunction cmp)
			{
				this.cmp = cmp;
			}

			// Calls a user supplied compare ScriptFunction
			internal sbyte userCompare (Hashtable elems, bool b1, object o1, uint i2)
			{
				bool b2 = !elems.ContainsKey (i2);
				if (b1 && b2)
					return 0;
				else if (b1)
					return 1;
				else if (b2)
					return -1;

				object o2 = elems [i2];
				if (o1 == null && o2 == null)
					return 0;
				if (o1 == null)
					return 1;
				else if (o2 == null)
					return -1;

				int res = Convert.ToInt32 (cmp.Invoke (null, o1, o2));
				if (res < 0)
					return -1;
				else if (res > 0)
					return 1;
				else
					return 0;
			}

			// Uses a built-in compare function
			internal static sbyte nativeCompare (Hashtable elems, bool b1, object o1_, uint i2)
			{
				bool b2 = !elems.ContainsKey (i2);
				if (b1 && b2)
					return 0;
				else if (b1)
					return 1;
				else if (b2)
					return -1;

				IComparable o1 = o1_ as IComparable;
				IComparable o2 = elems [i2] as IComparable;
				if (o1 == null && o2 == null)
					return 0;
				if (o1 == null)
					return 1;
				else if (o2 == null)
					return -1;

				return (sbyte) Relational.JScriptCompare (o1, o2);
			}

			internal static void swap (Hashtable elems, uint i1, uint i2)
			{
				object temp = elems [i1];
				elems [i1] = elems [i2];
				elems [i2] = temp;
			}

			internal static uint partition (Hashtable elems, uint left, uint right, CompareDelegate cmp)
			{
				uint pivotIndex = left + (uint) MathObject.random_gen.Next ((int) right - (int) left + 1);
				bool pivotMissing = !elems.ContainsKey (pivotIndex);
				object pivotValue = elems [pivotIndex];
				swap (elems, pivotIndex, right);
				uint storeIndex = left;
				for (uint i = left; i < right; i++)
					if (cmp (elems, pivotMissing, pivotValue, i) >= 0)
						swap (elems, storeIndex++, i);
				swap (elems, right, storeIndex);
				return storeIndex;
			}

			internal static void qsort (Hashtable elems, uint beg, uint end, CompareDelegate cmp)
			{
				if (end > beg) {
					uint index = partition (elems, beg, end, cmp);
					if (index > 0)
						qsort (elems, beg, index - 1, cmp);
					qsort (elems, index + 1, end, cmp);
				}
			}
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

			uint old_length = (uint) array_obj.length;
			start = (long) start;
			if (start < 0)
				start = Math.Max (start + old_length, 0);
			else
				start = Math.Min (old_length, start);

			deleteCnt = (long) deleteCnt;
			deleteCnt = Math.Min (Math.Max (deleteCnt, 0), old_length - start);

			uint arg_length = (uint) args.Length;
			long add_length = (long) ((long) arg_length - (uint) deleteCnt);
			add_length = (long) Math.Max (add_length, -((long) old_length));
			long del_length = -add_length;
			uint new_length = (uint) ((long) old_length + add_length);

			long i, j, m;
			// First let's make some free space for the new items (if needed)
			if (add_length > 0) {
				i = (long) old_length - 1;
				j = (uint) (i + add_length);
				for (; i >= start; i--, j--)
					elems [(uint) j] = elems [(uint) i];
			}

			// Then insert the new items in the now free space / replace existing ones
			j = m = 0;
			long old_start = (long) (start + add_length);
			for (i = (long) start; j < arg_length; i++, j++) {
				if (i >= old_start && elems.ContainsKey ((uint) i)) {
					del_elems [(uint) m] = elems [(uint) i];
					m++;
					elems.Remove ((uint) i);
				}

				if (j < arg_length)
					elems [(uint) i] = args [(uint) j];
			}

			// Finally, delete elements which have no replacement elements
			if (add_length < 0) {
				uint last_elem_idx = (uint) (i + del_length);
				for (uint k = 0; k < del_length; i++, j++, k++) {
					if (elems.ContainsKey ((uint) i)) {
						del_elems [(uint) m] = elems [(uint) i];
						m++;
						elems.Remove ((uint) i);
					}
				}

				// And move up trailing elements
				uint l = (uint) (last_elem_idx - del_length);
				for (uint k = last_elem_idx; l < old_length; k++, l++) {
					if (elems.ContainsKey (k))
						elems [l] = elems [k];
					else if (elems.ContainsKey (l))
						elems.Remove (l);
				}
			}

			array_obj.length = new_length;
			result.length = (uint) deleteCnt;
			return result;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_toLocaleString)]
		public static string toLocaleString (object thisObj)
		{
			// TODO: Shouldn't this be generic!?
			SemanticAnalyser.assert_type (thisObj, typeof (ArrayObject));
			ArrayObject array_obj = (ArrayObject) thisObj;

			string separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";

			Hashtable elems = array_obj.elems;
			uint n = (uint) array_obj.length;
			StringBuilder str = new StringBuilder ();
			bool first = true;

			for (uint i = 0; i < n; i++) {
				ScriptObject elem = (ScriptObject) Convert.ToObject (elems [i], null);
				if (!first && elem != null)
					str.Append (separator);
				first = false;
				if (elem != null)
					str.Append (Convert.ToString (elem.CallMethod ("toLocaleString", new object [] { } )));
			}
			return str.ToString ();
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

			uint old_length = (uint) array_obj.length;
			uint arg_length = (uint) args.Length;
			uint new_length = old_length + arg_length;

			if (arg_length > 0) {
				// First let's make some free space for the new items
				long i = (long) old_length - 1;
				long j = i + (long) arg_length;
				for (; i >= 0; i--, j--)
					elems [(uint) j] = elems [(uint) i];

				// Then insert the new items in the now free space
				for (; j >= 0; j--)
					elems [(uint) j] = args [(uint) j];
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
