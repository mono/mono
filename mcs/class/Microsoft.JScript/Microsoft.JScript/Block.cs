//
// Block.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	public class Block : AST
	{
		public override object Visit (Visitor v, object args)
		{
			return v.VisitBlock (this, args);
		}
	}
}