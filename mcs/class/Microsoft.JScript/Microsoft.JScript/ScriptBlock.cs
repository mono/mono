//
// ScriptBlock.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp
{
	public class ScriptBlock : AST
	{
		internal override object Visit (Visitor v, object args)
		{
			return v.VisitScriptBlock (this, args);
		}
	}
}