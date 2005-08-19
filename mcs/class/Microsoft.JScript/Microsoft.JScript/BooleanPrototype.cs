//
// BooleanPrototype.cs:
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

	public class BooleanPrototype : BooleanObject {

		internal static BooleanPrototype Proto = new BooleanPrototype (null, null);

		public static BooleanConstructor constructor {
			get { return BooleanConstructor.Ctr; }
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Boolean_toString)]
		public static string toString (object thisObj)
		{
			if (!Convert.IsBoolean (thisObj))
				throw new JScriptException (JSError.BooleanExpected);

			if (thisObj is bool)
				return (bool) thisObj ? "true" : "false";

			BooleanObject bo = thisObj as BooleanObject;
			return bo.value ? "true" : "false";
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Boolean_valueOf)]
		public static object valueOf (object thisObj)
		{
			if (!Convert.IsBoolean (thisObj))
				throw new JScriptException (JSError.BooleanExpected);

			if (thisObj is bool)
				return thisObj;

			BooleanObject bo = thisObj as BooleanObject;
			return bo.value;
		}

		protected BooleanPrototype (ObjectPrototype prototype, Type baseType)
		{
		}
	}
}


