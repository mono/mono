//
// Block.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System.Collections;
using System.Text;
using System;

namespace Microsoft.JScript {

	public class Block : AST {

		internal ArrayList Elements;
		internal AST parent;

		internal Block (AST parent)
		{
			this.parent = parent;
			Elements = new ArrayList ();
		}

		internal void Add (AST e)
		{
			Elements.Add (e);
		}

		public override string ToString ()
		{
			System.Console.WriteLine (Elements.Count);

			StringBuilder sb = new StringBuilder ();

			foreach (AST a in Elements)
				if (a != null)
					sb.Append (a.ToString () + " ");

			return sb.ToString ();
		}

		internal override void Emit (EmitContext ec)
		{
			int i, size = Elements.Count;

			for (i = 0; i < size; i++)
				((AST) Elements [i]).Emit (ec);
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;
			int i, size = Elements.Count;

			System.Console.WriteLine ("Block::Resolve");

			for (i = 0; i < size; i++)
				r &= ((AST) Elements [i]).Resolve (context);

			return r;
		}
	}
}
