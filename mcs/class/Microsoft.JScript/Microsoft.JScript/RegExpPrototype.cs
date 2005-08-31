//
// RegExpPrototype.cs
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
// (C) Copyright 2005, Novell Inc (http://novell.com)
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
using System.Text.RegularExpressions;
using System.Collections;

namespace Microsoft.JScript {	

	public class RegExpPrototype : JSObject	{

		internal static RegExpPrototype Proto = new RegExpPrototype ();

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.RegExp_compile)]
		public static RegExpObject compile (object thisObj, object source, object flags)
		{
			//
			// Note: We always compile RegExp internals so all this method is useful for is for
			// changing the properties of the otherwise immutable RegExp objects.
			//
			RegExpObject re = Convert.ToRegExp (thisObj);
			string flag_str = Convert.ToString (flags);

			re.Initialize (Convert.ToString (source),
				flag_str.IndexOfAny (new char [] { 'i' }) > -1,
				flag_str.IndexOfAny (new char [] { 'g' }) > -1,
				flag_str.IndexOfAny (new char [] { 'm' }) > -1);
			return re;
		}

		public static RegExpConstructor constructor {
			get { return RegExpConstructor.Ctr; }
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.RegExp_exec)]
		public static object exec (object thisObj, object input)
		{
			RegExpObject re = Convert.ToRegExp (thisObj);
			string str = null;
			if (input == null) {
				RegExpConstructor ctr = RegExpConstructor.Ctr;
				str = Convert.ToString (ctr.GetField ("$_").GetValue ("$_"));
			} else
				str = Convert.ToString (input);
			bool global = re.global;
			int lastIndex = global ? (int) ((double) re.lastIndex) : 0;
			bool success = lastIndex >= 0 && lastIndex <= str.Length;

			Match md = null;
			if (success) {
				md = re.regex.Match (str, lastIndex);
				success = md.Success;
			}

			if (!success) {
				re.lastIndex = 0;
				return DBNull.Value;
			}

			int index = md.Index;
			int endIndex = index + md.Length;
			if (global)
				re.lastIndex = endIndex;
			RegExpConstructor.UpdateLastMatch (md, str);

			GroupCollection caps = md.Groups;
			uint len = (uint) caps.Count;
			RegExpMatch result = new RegExpMatch ();

			result.AddField ("index", index);
			result.AddField ("input", input);
			result.length = len;
			for (uint j = 0; j < len; j++)
				result.elems [j] = caps [(int) j].Value;

			return result;
		}


		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.RegExp_test)]
		public static bool test (object thisObj, object input)
		{
			return exec (thisObj, input) != DBNull.Value;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.RegExp_toString)]
		public static string toString (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (RegExpObject));
			RegExpObject re = (RegExpObject) thisObj;
			return re.ToString ();
		}
	}
}
