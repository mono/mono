//
// SymbolTable.cs: 
//
// Author:
//	Cesar Lopez Nataren
//
// (C) 2003, Cesar Lopez Nataren, <cesar@ciencias.unam.mx>
//


using System;
using System.Collections;

namespace Microsoft.JScript
{
	internal class SymbolTable
	{
		internal Hashtable symbols;

		internal SymbolTable ()
		{
			symbols = new Hashtable ();
		}
		
	
		internal void Add (string id, VariableDeclaration d)
		{
			symbols.Add (id, d);
		}


		internal VariableDeclaration Retrieve (string id)
		{
			return ((VariableDeclaration) symbols [id]);
		}

		
		internal bool Contains (string id)
		{
			return symbols.ContainsKey (id);
		}
	}
}
