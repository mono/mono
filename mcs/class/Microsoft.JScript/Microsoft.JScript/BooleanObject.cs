//
// BooleanObject.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
// (C) 2005, Novell, Inc.
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

	public class BooleanObject : JSObject {

		internal bool value;

		internal BooleanObject ()
		{
			this.value = false;
		}

		internal BooleanObject (object value)
		{
			this.value = Convert.ToBoolean (value);
		}

		public new Type GetType ()
		{
			throw new NotImplementedException ();
		}

		protected BooleanObject (ScriptObject prototype, Type subType)
		{
			throw new NotImplementedException ();
		}

		internal override object GetDefaultValue (Type hint, bool avoid_toString)
		{
			if (hint == typeof (string))
				return BooleanPrototype.toString (this);
			else
				return value;
		}
	}
}
