//
// ExecutionContext.cs: The stack of possible executions environments.
//
// Author:
//	Cesar Lopez Nataren
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

using System.Collections;
using System.Text;

namespace Microsoft.JScript {

	internal class IdentificationTable {

		internal Stack stack;

		internal IdentificationTable ()
		{
			stack = new Stack ();
			stack.Push (new SymbolTable (null));
		}
		
		internal void OpenBlock ()
		{
			SymbolTable parent = (SymbolTable) stack.Peek ();
			stack.Push (new SymbolTable (parent));
		}

		internal void CloseBlock ()
		{
			stack.Pop ();
		}

		internal void Enter (string id, object decl)
		{			
			((SymbolTable) stack.Peek ()).Add (id , decl);
		}

		internal void Remove (string id)
		{
			((SymbolTable) stack.Peek ()).Remove (id);
		}
		//
		// It'll return the object asociated with the 'id', if found.
		//
		internal object Contains (string id)
		{
			SymbolTable parent, current_scope = (SymbolTable) stack.Peek ();
			object found = current_scope.Contains (id);

			if (found == null) {
				parent = current_scope.parent;

				if (parent != null)
					found = parent.Contains (id);
			}
			return found;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			int i, size = stack.Count;

			for (i = 0; i < size; i++)
				sb.Append (stack.Pop ().ToString ());

			return sb.ToString ();
		}

		internal int num_of_locals {
			get { return  ((SymbolTable) stack.Peek ()).size; }
		}

		internal DictionaryEntry [] current_locals {
			get { return  ((SymbolTable) stack.Peek ()).current_symbols; }
		}
	}
}
