//
// ASTList.cs: Representation of a collection of source elements 
//             that form an Ecmascript program.
//
// Author: 
//	Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System.Collections;
using System.Text;
using System;

namespace Microsoft.JScript.Tmp {

	public class ASTList : AST {

		internal ArrayList elems;

		internal ASTList ()
		{
			elems = new ArrayList ();
		}

		internal ASTList Add (AST elem)
		{
			elems.Add (elem);
			return this;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			foreach (AST ast in elems)
				sb.Append (ast.ToString () + "\n");

			return sb.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}
	}
}
