//
// SymbolTable.cs: 
//
// Author:
//	Cesar Lopez Nataren
//
// (C) 2003, Cesar Lopez Nataren, <cesar@ciencias.unam.mx>
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
using  System.Text;

namespace Microsoft.JScript {

	internal class SymbolTable {

		internal SymbolTable parent;
		internal Hashtable symbols;
		
		internal SymbolTable (SymbolTable parent)
		{
			symbols = new Hashtable ();
			this.parent = parent;
		}
		
		internal void Add (string id, object d)
		{
			symbols.Add (id, d);
		}

		internal void Remove (string id)
		{
			symbols.Remove (id);
		}

		internal object Contains (string id)
		{
			return symbols [id];
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			ICollection keys = symbols.Keys;

			foreach (object o in keys)
				sb.Append (o.ToString ());

			return sb.ToString ();
		}

		internal int size {
			get { return symbols.Count; }
		}

		internal DictionaryEntry [] current_symbols {
			get {
				int n = symbols.Count;
				if (n == 0)
					return null;
				else {
					DictionaryEntry [] e = new DictionaryEntry [symbols.Count];
					symbols.CopyTo (e, 0);
					return e;
				}
			}
		}
	}
}
