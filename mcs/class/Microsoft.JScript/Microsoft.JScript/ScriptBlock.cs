//
// ScriptBlock.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	public class ScriptBlock : AST
	{
		public override object Visit (Visitor v, object args)
		{
			return v.VisitScriptBlock (this, args);
		}
	}
}