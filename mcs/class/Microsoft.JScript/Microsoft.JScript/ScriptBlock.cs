//
// ScriptBlock.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp {

	public class ScriptBlock : AST {

		internal Block SrcElems;

		internal ScriptBlock ()
		{
			SrcElems = new Block ();
		}

		internal void Add (AST e)
		{
			SrcElems.Add (e);
		}

		public override string ToString ()
		{
			return SrcElems.ToString ();
		}
	}
}