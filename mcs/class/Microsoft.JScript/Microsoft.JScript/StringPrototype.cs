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

using System;
using Microsoft.JScript.Vsa;
using System.Globalization;

namespace Microsoft.JScript {
	
	public class StringPrototype : StringObject {

		/* Note that the implementation of this HTML tag stuff is pretty dumb.
		 * It does not do any escaping of HTML characters like '<', '>' or '"' which means that you can do
		 * HTML injection via for example "foo".anchor("NAME\" style=\"font-size: 500pt;") which will result
		 * in <A NAME="NAME" style="font-size: 500pt;">, however Mozilla, Internet Explorer and Opera all do
		 * the same thing and I could not find any standard against this behavior. */

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_anchor)]
		public static string anchor (object thisObj, object anchorName)
		{
			return "<A NAME=\"" + Convert.ToString (anchorName) + "\">" + Convert.ToString (thisObj) + "</A>";
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_big)]
		public static string big (object thisObj)
		{
			return "<BIG>" + Convert.ToString (thisObj) + "</BIG>";
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_blink)]
		public static string blink (object thisObj)
		{
			return "<BLINK>" + Convert.ToString (thisObj) + "</BLINK>";
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_bold)]
		public static string bold (object thisObj)
		{
			return "<B>" + Convert.ToString (thisObj) + "</B>";
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_charAt)]
		public static string charAt (object thisObj, double pos)
		{
			int _pos = Convert.ToInt32 (pos);
			string string_obj = Convert.ToString (thisObj);

			if (_pos < 0 || _pos >= string_obj.Length)
				return "";

			return string_obj.Substring (_pos, 1);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_charCodeAt)]
		public static object charCodeAt (object thisObj, double arg)
		{
			int pos = Convert.ToInt32 (arg);
			string string_obj = Convert.ToString (thisObj);

			if (pos < 0 || pos >= string_obj.Length)
				return Double.NaN;

			return string_obj [pos];
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject | JSFunctionAttributeEnum.HasVarArgs, JSBuiltin.String_concat)]
		public static string concat (object thisObj, params object [] args)
		{
			throw new NotImplementedException ();
		}


		public static StringConstructor constructor {
			get { return StringConstructor.Ctr; }
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_fixed)]
		public static string @fixed (object thisObj)
		{
			return "<TT>" + Convert.ToString (thisObj) + "</TT>";
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_fontcolor)]
		public static string fontcolor (object thisObj, object colorName)
		{
			return "<FONT COLOR=\"" + Convert.ToString (colorName) + "\">" + Convert.ToString (thisObj) + "</FONT>";
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_fontsize)]
		public static string fontsize (object thisObj, object fontsize)
		{
			return "<FONT SIZE=\"" + Convert.ToString (fontsize) + "\">" + Convert.ToString (thisObj) + "</FONT>";
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_indexOf)]
		public static int indexOf (object thisObj, object searchString, double position)
		{
			string string_obj = Convert.ToString (thisObj);
			string search_obj = Convert.ToString (searchString);
			int pos = (int) position;
			if (pos < 0)
				pos = 0;
			else if (pos > string_obj.Length)
				pos = string_obj.Length;
			return string_obj.IndexOf (search_obj, pos);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_italics)]
		public static string italics (object thisObj)
		{
			return "<I>" + Convert.ToString (thisObj) + "</I>";
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_lastIndexOf)]
		public static int lastIndexOf (object thisObj, object searchString, object position)
		{
			string string_obj = Convert.ToString (thisObj);
			string search_obj = Convert.ToString (searchString);
			int pos;
			if (position == null)
				pos = string_obj.Length;
			else {
				pos = Convert.ToInt32 (position);
				if (pos < 0)
					pos = 0;
				else if (pos > string_obj.Length)
					pos = string_obj.Length;
			}
			int result = string_obj.LastIndexOf (search_obj, pos);
			// string:LastIndexOf ignores matches at string start if pos is 0
			if (result == -1 && pos == 0 && string_obj.StartsWith (search_obj))
				return 0;
			return result;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_link)]
		public static string link (object thisObj, object linkRef)
		{
			return "<A HREF=\"" + Convert.ToString (linkRef) + "\">" + Convert.ToString (thisObj) + "</A>";
		}

		[MonoTODO ("I18N Needs checking -- contact flgr@ccan.de for details")]
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_localeCompare)]
		public static int localeCompare (object thisObj, object thatObj)
		{
			// TODO FIXME I18N: Verify that we do the right thing even in border cases.
			string string_a = Convert.ToString (thisObj);
			string string_b = Convert.ToString (thatObj);
			int caseless_result = String.Compare (string_a, string_b, true);
			/* I have no idea if this is enough to fix the behavior in all cases, but it at least makes
			 * localeCompare("abc", "ABC") work as in MS JS.NET -- this will likely be revised after
			 * more testing. */
			if (caseless_result == 0)
				return -String.Compare (string_a, string_b, false);
			else
				return String.Compare (string_a, string_b, false);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject | JSFunctionAttributeEnum.HasEngine, JSBuiltin.String_match)]
		public static object match (object thisObj, VsaEngine engine, object regExp)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_replace)]
		public static string replace (object thisObj, object regExp, object replacement)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject | JSFunctionAttributeEnum.HasEngine, JSBuiltin.String_search)]
		public static int search (object thisObj, VsaEngine engine, object regExp)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_slice)]
		public static string slice (object thisObj, double start, object end)
		{
			string string_obj = Convert.ToString (thisObj);
			int string_len = string_obj.Length;
			int _start, _end;

			if (start > string_len)
				_start = string_len;
			else {
				_start = (int) start;
				if (_start < 0)
					_start += string_len;
			}

			if (end == null)
				_end = string_len;
			else {
				_end = (int) (double) end;

				if (_end < 0)
					_end += string_len;
				else if (_end > string_len)
					_end = string_len;
			}

			if (_end < _start)
				_end = _start;

			return string_obj.Substring (_start, _end - _start);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_small)]
		public static string small (object thisObj)
		{
			return "<SMALL>" + Convert.ToString (thisObj) + "</SMALL>";
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject | JSFunctionAttributeEnum.HasEngine, JSBuiltin.String_split)]
		public static ArrayObject split (object thisObj, VsaEngine engine,
						 object separator, object limit)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_strike)]
		public static string strike (object thisObj)
		{
			return "<STRIKE>" + Convert.ToString (thisObj) + "</STRIKE>";
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_sub)]
		public static string sub (object thisObj)
		{
			return "<SUB>" + Convert.ToString (thisObj) + "</SUB>";
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_substr)]
		public static string substr (object thisObj, double start, object count)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_substring)]
		public static string substring (object thisObj, double start, object end)
		{
			string string_obj = Convert.ToString (thisObj);
			int string_len = string_obj.Length;
			int _start, _end;

			if (start == Double.NaN || start < 0)
				_start = 0;
			else if (start > string_len)
				_start = string_len;
			else
				_start = (int) start;

			if (end == null)
				_end = string_len;
			else {
				_end = (int) (double) end;

				if (_end == Double.NaN || _end < 0)
					_end = 0;
				else if (_end > string_len)
					_end = string_len;
			}

			if (_end < _start) {
				int temp = _start;
				_start = _end;
				_end = temp;
			}
			return string_obj.Substring (_start, _end - _start);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_sup)]
		public static string sup (object thisObj)
		{
			return "<SUP>" + Convert.ToString (thisObj) + "</SUP>";
		}

		/* TODO FIXME I18N: Somebody who is familar with locales should check if the definition of these really is
		 * this simple. */

		[MonoTODO ("I18N Needs checking -- contact flgr@ccan.de for details")]
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_toLocaleLowerCase)]
		public static string toLocaleLowerCase (object thisObj)
		{
			string string_obj = Convert.ToString (thisObj);
			return string_obj.ToLower ();
		}

		[MonoTODO ("I18N Needs checking -- contact flgr@ccan.de for details")]
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_toLocaleUpperCase)]
		public static string toLocaleUpperCase (object thisObj)
		{
			string string_obj = Convert.ToString (thisObj);
			return string_obj.ToUpper ();
		}

		[MonoTODO ("I18N Needs checking -- contact flgr@ccan.de for details")]
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_toLowerCase)]
		public static string toLowerCase (object thisObj)
		{
			string string_obj = Convert.ToString (thisObj);
			return string_obj.ToLower (CultureInfo.InvariantCulture);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_toString)]
		public static string toString (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (StringObject));
			StringObject str_obj = thisObj as StringObject;
			return str_obj.value;
		}

		[MonoTODO ("I18N Needs checking -- contact flgr@ccan.de for details")]
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_toUpperCase)]
		public static string toUpperCase (object thisObj)
		{
			string string_obj = Convert.ToString (thisObj);
			return string_obj.ToUpper (CultureInfo.InvariantCulture);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_valueOf)]
		public static object valueOf (object thisObj)
		{
			return toString (thisObj);
		}
	}
}
