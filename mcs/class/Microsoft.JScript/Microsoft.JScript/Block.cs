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

		internal ArrayList elems;

		internal Block (AST parent)
		{
			this.parent = parent;
			elems = new ArrayList ();
		}

		internal void Add (AST e)
		{
			elems.Add (e);
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			foreach (AST a in elems)
				if (a != null)
					sb.Append (a.ToString () + " ");

			return sb.ToString ();
		}

		internal override void Emit (EmitContext ec)
		{
			int i, n = elems.Count;
			object e;
			
			for (i = 0; i < n; i++) {
				e = elems [i];
				if (e is FunctionDeclaration)
					((FunctionDeclaration) e).Emit (ec);
			}

			for (i = 0; i < n; i++) {
				e = elems [i];
				if (!(e is FunctionDeclaration))
					((AST) e).Emit (ec);
			}
		}

		internal override bool Resolve (IdentificationTable context)
		{
			AST e;
			bool r = true;
			int i, n = elems.Count;

			for (i = 0; i < n; i++) {
				e = (AST) elems [i];
				r &= e.Resolve (context);
			}
			return r;			
		}
	}
}
