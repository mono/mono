//
// Statement.cs:
//
// Author:
//	Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp
{
	public class Statement : AST
	{
		public Statement ()
		{}

		internal override object Visit (Visitor v, object args)
		{
			return null;
		}
	}
}	