//
// VariableStatement.cs: The AST representation of a VariableStatement.
//
// Author:
//	Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System.Collections;
using System.Text;

namespace Microsoft.JScript {

	public class VariableStatement : Statement {

		internal ArrayList varDecls;

		internal VariableStatement ()
		{
			varDecls = new ArrayList ();
		}


		internal void Add (VariableDeclaration varDecl)
		{
			varDecls.Add (varDecl);
		}


		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			foreach (VariableDeclaration varDecl in varDecls)
				sb.Append (varDecl.ToString () + " ");

			return sb.ToString ();
		}
	}
}