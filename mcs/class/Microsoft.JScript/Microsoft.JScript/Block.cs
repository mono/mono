//
// Block.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System.Collections;
using System.Text;

namespace Microsoft.JScript {

	public class Block : AST {

		internal ArrayList Elements;

		internal Block ()
		{
			Elements = new ArrayList ();
		}

		internal void Add (AST e)
		{
			Elements.Add (e);
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			foreach (AST a in Elements)
				sb.Append (a.ToString () + " ");

			return sb.ToString ();
		}
	}
}