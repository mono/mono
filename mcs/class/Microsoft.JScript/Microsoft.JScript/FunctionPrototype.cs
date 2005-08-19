//
// FunctionPrototype.cs:
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

namespace Microsoft.JScript {

	public class FunctionPrototype : ScriptFunction	{

		internal static FunctionPrototype Proto = new FunctionPrototype ();

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Function_apply)]
		public static object apply (object thisObj, object thisArg, object argArray)
		{
			object [] args = (object []) Convert.ToNativeArray (argArray, typeof (object).TypeHandle);
			return call (thisObj, thisArg, args);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject | JSFunctionAttributeEnum.HasVarArgs, JSBuiltin.Function_call)]
		public static object call (object thisObj, object thisArg, params object [] args)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (ScriptFunction));
			ScriptFunction fun = (ScriptFunction) thisObj;
			return fun.Invoke (thisArg, args);
		}

		public static FunctionConstructor constructor {
			get { return FunctionConstructor.Ctr; }
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Function_toString)]
		public static string toString (object thisObj)
		{
			if (thisObj is ScriptFunction)
				return (((ScriptFunction) thisObj).ToString ());

			throw new JScriptException (JSError.FunctionExpected);
		}
	}
}
