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

		internal ArrayList var_decls;

		internal VariableStatement ()
		{
			var_decls = new ArrayList ();
		}


		internal void Add (VariableDeclaration varDecl)
		{
			var_decls.Add (varDecl);
		}


		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			foreach (VariableDeclaration var_decl in var_decls)
				sb.Append (var_decl.ToString () + " ");

			return sb.ToString ();
		}
	}
}