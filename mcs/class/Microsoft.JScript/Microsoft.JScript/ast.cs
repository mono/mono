//
// ast.cs: Base class for the EcmaScript program tree representation.
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp
{
	public abstract class AST
	{
		internal abstract object Visit (Visitor v, object obj);
	}
}