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
using System;

namespace Microsoft.JScript {

	public class VariableStatement : AST {

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

		internal override void Emit (EmitContext ec)
		{
			int i, size = var_decls.Count;

			for (i = 0; i < size; i++)
				((VariableDeclaration) var_decls [i]).Emit (ec);
		}

		internal override bool Resolve (IdentificationTable context)
		{
			VariableDeclaration tmp_decl;
			int i, size = var_decls.Count;
			bool res = true;
			AST init_val;

			for (i = 0; i < size; i++) {
				tmp_decl = (VariableDeclaration) var_decls [i];
				init_val = tmp_decl.InitValue;

				if (init_val != null)
					res = init_val.Resolve (context);

				if (res)
					context.Enter (tmp_decl.Id, tmp_decl);
			}

			return true;
		}
	}
}
