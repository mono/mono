//
// VariableStatement.cs: The AST representation of a VariableStatement.
//
// Author:
//	Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections;
using System.Text;
using System;

namespace Microsoft.JScript {

	public class VariableStatement : AST {

		internal ArrayList var_decls;

		internal VariableStatement (AST parent, int line_number)
		{
			this.parent = parent;
			this.line_number = line_number;
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
			int i, n = var_decls.Count;
			bool r = true;

			for (i = 0; i < n; i++) {
				tmp_decl = (VariableDeclaration) var_decls [i];
				r &= tmp_decl.Resolve (context);
			}
			return r;
		}
	}
}
