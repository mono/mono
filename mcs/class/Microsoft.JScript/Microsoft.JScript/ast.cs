//
// ast.cs: Base class for the EcmaScript program tree representation.
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	public abstract class AST
	{
		public abstract object Visit (Visitor v, object obj);
	}
}