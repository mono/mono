//
// ast.cs: Base class for the EcmaScript program tree representation.
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript {

	public abstract class AST {

		internal virtual void Emit (EmitContext ec)
		{
		}
	}
}