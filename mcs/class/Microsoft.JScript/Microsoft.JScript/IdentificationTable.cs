//
// ExecutionContext.cs: The stack of possible executions environments.
//
// Author:
//	Cesar Lopez Nataren
//
// (C) 2003, Cesar Lopez Nataren
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
