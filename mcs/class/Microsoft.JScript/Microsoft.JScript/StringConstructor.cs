//
// StringConstructor.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
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

namespace Microsoft.JScript {

	public class StringConstructor : ScriptFunction {

		internal static StringConstructor Ctr = new StringConstructor ();

		internal StringConstructor ()
		{
			_prototype = StringPrototype.Proto;
			_length = 1;
			name = "String";
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs)]
		public new StringObject CreateInstance (params Object [] args)
		{
			if (args == null || args.Length == 0)
				return new StringObject ();

			object arg = args [0];

			if (arg == null)
				return new StringObject ("undefined");
			else
				return new StringObject (Convert.ToString (arg));
		}


		public string Invoke (Object arg)
		{
			if (arg is Object []) {
				Object [] args = (Object []) arg;
				if (args.Length == 0)
					return "";
				else
					return Invoke (args [0]);
			} else
				return Convert.ToString (arg);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs, JSBuiltin.String_fromCharCode)]
		public static String fromCharCode (params Object [] args)
		{
			string result = "";
			foreach (object arg in args)
				result += (char) Convert.ToUint16 (arg);
			return result;
		}
	}
}
