//
// ScriptBlock.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript {

	public class ScriptBlock : AST {

		internal Block src_elems;

		internal ScriptBlock ()
		{
			src_elems = new Block ();
		}

		internal void Add (AST e)
		{
			src_elems.Add (e);
		}

		public override string ToString ()
		{
			return src_elems.ToString ();
		}

		internal override void Emit (EmitContext ec)
		{
			src_elems.Emit (ec);
		}

		internal override bool Resolve (IdentificationTable context)
		{
			System.Console.WriteLine ("ScriptBlock::Resolve");
			return src_elems.Resolve (context);
		}
	}
}
