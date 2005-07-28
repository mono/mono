//
// ArrayObject.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
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

namespace Microsoft.JScript {

	public class ArrayObject : JSObject {

		private object _length;

		public virtual object length {
			get { return _length; }
			set {
				uint new_length = Convert.ToUint32 (value);
				uint old_length = (uint) _length;

				for (uint i = new_length; i < old_length; i++)
					if (elems.ContainsKey (i))
						elems.Remove (i);

				_length = new_length;
			}
		}

		internal ArrayObject ()
		{
			_length = (uint) 0;
		}

		internal ArrayObject (object o)
		{
			IConvertible ic = o as IConvertible;
			TypeCode tc = Convert.GetTypeCode (o, ic);

			try {
				if (Convert.IsNumberTypeCode (tc)) {
					uint size = Convert.ToUint32 (o);
					if (size >= 0) {
						_length = size;
						return;
					} else {
						Console.WriteLine ("size = {0}", size);
						throw new JScriptException (JSError.ArrayLengthConstructIncorrect);
					}
				}
			} catch (FormatException) { /* OK */ }

			elems = new Hashtable ();
			_length = (uint) 1;
			elems.Add (0, o);
		}

		internal ArrayObject (params object [] args)
		{
			if (args != null) {
				uint size = (uint) args.Length;
				_length = size;
				elems = new Hashtable ();

				uint idx = 0;
				foreach (object o in args) {
					elems.Add (idx, o);
					idx++;
				}
			} else
				throw new Exception ("args can't be null");
		}

		internal Hashtable Elements {
			get { return elems; }
		}

		protected void SpliceSlowly (uint start, uint deleteCount, object [] args, ArrayObject outArray, uint oldLength, uint newLength)
		{
			ArrayPrototype.splice (outArray, null, start, deleteCount, args);
		}

		internal override object GetDefaultValue (Type hint, bool avoid_toString)
		{
			return ArrayPrototype.toString (this);
		}
	}
}
