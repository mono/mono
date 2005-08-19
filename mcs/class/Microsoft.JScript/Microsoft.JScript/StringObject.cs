//
// StringObject.cs:
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

	public class StringObject : JSObject {

		internal string value;

		internal StringObject ()
		{
			this.value = "";
		}

		internal StringObject (string value)
		{
			this.value = value;
		}

		public int length {
			get { return value.Length; }
		}

		public override bool Equals (Object obj)
		{
			StringObject other = obj as StringObject;
			if (other == null)
				return false;
			else
				return other.value == this.value;
		}

		public override int GetHashCode ()
		{
			return value.GetHashCode ();
		}

		public new Type GetType ()
		{
			throw new NotImplementedException ();
		}

		internal override object GetDefaultValue (Type hint, bool avoid_toString)
		{
			return value;
		}
	}
}
