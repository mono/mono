//
// ExecutionContext.cs: The stack of possible executions environments.
//
// Author:
//	Cesar Lopez Nataren
//
// (C) 2003, Cesar Lopez Nataren
//

using System.Collections;

namespace Microsoft.JScript
{
	internal class IdentificationTable
	{
		internal Stack stack;

		internal IdentificationTable ()
		{
			stack = new Stack ();
			stack.Push (new SymbolTable ());
		}

		
		internal void OpenBlock ()
		{
			stack.Push (new SymbolTable ());
		}


		internal void CloseBlock ()
		{
			stack.Pop ();
		}


		internal void Enter (string id, VariableDeclaration decl)
		{
			((SymbolTable) stack.Peek ()).Add (id , decl);
		}


		internal VariableDeclaration Retrieve (string id)
		{
			return ((SymbolTable) stack.Peek ()).Retrieve (id);
		}


		internal bool Contains (string id)
		{
			return ((SymbolTable) stack.Peek ()).Contains (id);
		}
	}
}
