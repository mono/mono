//
// Block.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System.Collections;

namespace Microsoft.JScript.Tmp {

	public class Block : AST {

		internal ArrayList Elements;

		internal void Add (AST e)
		{
			Elements.Add (e);
		}
	}
}