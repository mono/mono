//
// ArrayLiteral.cs:
//
// Author: 
//	Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp
{
	public class ArrayLiteral : AST
	{
		ASTList elems;

		public ArrayLiteral (Context context, ASTList elems)
		{}

		
		internal override object Visit (Visitor v, object args)
		{
			return v.VisitArrayLiteral (this, args);
		}
	}
}