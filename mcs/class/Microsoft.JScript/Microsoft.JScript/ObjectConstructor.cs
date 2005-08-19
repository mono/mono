//
// ObjectConstructor.cs:
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

	public class ObjectConstructor : ScriptFunction {

		internal static ObjectConstructor Ctr = new ObjectConstructor ();

		internal ObjectConstructor ()
		{
			_prototype = ObjectPrototype.Proto;
			_length = 1;
			name = "Object";
		}

		public JSObject ConstructObject ()
		{
			return new ObjectPrototype ();
		}
		
		[JSFunctionAttribute(JSFunctionAttributeEnum.HasVarArgs)]
		public new Object CreateInstance (params object [] args)
		{
			if (args == null || args.Length == 0)
				return ConstructObject ();
			else {
				object value = args [0];

				if (value == null || value == DBNull.Value)
					return ConstructObject ();
				else
					return Convert.ToObject (value, null);
			}
		}

		[JSFunctionAttribute(JSFunctionAttributeEnum.HasVarArgs)]
		public Object Invoke(params Object[] args)
		{
			if (args == null || args.Length == 0)
				return ConstructObject ();
			else {
				object arg = args [0];
				if (arg == null || arg == DBNull.Value)
					return ConstructObject ();
				else
					return Convert.ToObject (arg, null);
			}
		}
	}
}
